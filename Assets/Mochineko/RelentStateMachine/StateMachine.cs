#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.Relent.Result;

namespace Mochineko.RelentStateMachine
{
    public sealed class StateMachine<TEvent, TContext> : IStateMachine<TEvent, TContext>
    {
        private readonly ITransitionMap<TEvent, TContext> transitionMap;
        public TContext Context { get; }
        private IState<TContext> state;

        public bool IsCurrentState<TState>()
            where TState : IState<TContext>
            => state is TState;

        public static async UniTask<IResult<StateMachine<TEvent, TContext>>> CreateAsync(
            ITransitionMap<TEvent, TContext> transitionMap,
            TContext context,
            CancellationToken cancellationToken)
        {
            var instance = new StateMachine<TEvent, TContext>(
                transitionMap,
                context);

            var initializeResult = await instance.state
                .EnterAsync(context, cancellationToken);
            if (initializeResult.Success)
            {
                return ResultFactory.Succeed(instance);
            }
            else if (initializeResult is IFailureResult initializeFailure)
            {
                return ResultFactory.Fail<StateMachine<TEvent, TContext>>(
                    $"Failed to enter initial state because of {initializeFailure.Message}.");
            }
            else
            {
                throw new ResultPatternMatchException(nameof(initializeResult));
            }
        }

        private StateMachine(
            ITransitionMap<TEvent, TContext> transitionMap,
            TContext context)
        {
            this.transitionMap = transitionMap;
            this.Context = context;
            this.state = this.transitionMap.InitialState;
        }

        public async UniTask<IResult> SendEventAsync(
            TEvent @event,
            CancellationToken cancellationToken)
        {
            IState<TContext> nextState;
            var transitResult = transitionMap.CanTransit(state, @event);
            if (transitResult is ISuccessResult<IState<TContext>> transitionSuccess)
            {
                nextState = transitionSuccess.Result;
            }
            else if (transitResult is IFailureResult<IState<TContext>> transitionFailure)
            {
                return ResultFactory.Fail(
                    $"Failed to transit state because of {transitionFailure.Message}.");
            }
            else
            {
                throw new ResultPatternMatchException(nameof(transitResult));
            }

            var exitResult = await state.ExitAsync(Context, cancellationToken);
            if (exitResult is IFailureResult exitFailure)
            {
                return ResultFactory.Fail(
                    $"Failed to exit current state:{state.GetType()} because of {exitFailure.Message}.");
            }

            var enterResult = await nextState.EnterAsync(Context, cancellationToken);
            if (enterResult is IFailureResult enterFailure)
            {
                return ResultFactory.Fail(
                    $"Failed to enter state:{nextState.GetType()} because of {enterFailure.Message}.");
            }

            state = nextState;

            return ResultFactory.Succeed();
        }

        public async UniTask<IResult> UpdateAsync(CancellationToken cancellationToken)
        {
            var updateResult = await state.UpdateAsync(Context, cancellationToken);
            if (updateResult is ISuccessResult)
            {
                return ResultFactory.Succeed();
            }
            else if (updateResult is IFailureResult updateFailure)
            {
                return ResultFactory.Fail(
                    $"Failed to update current state:{state.GetType()} because of {updateFailure.Message}.");
            }
            else
            {
                throw new ResultPatternMatchException(nameof(updateResult));
            }
        }

        public void Dispose()
        {
            transitionMap.Dispose();
        }
    }
}