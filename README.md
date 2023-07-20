# RelentStateMachine

An asynchronous-based finite state machine for Unity.

## Features

- Asynchronous-based ([UniTask](https://github.com/Cysharp/UniTask)-based) operations
- Thread-safe
- Null-safe
- Type-safe
- Be able to define events by any generic types
- Be able to hold any generic context data between states
- Immutable state transitions at runtime
- Do not depend on `MonoBehaviour` (UnityEngine)
- Interface-based abstractions

## Available State Machine

- Finite State Machine
- Stack-based State Machine

## How to import by Unity Package Manager

Add following dependencies to your `/Packages/manifest.json`.

```json
{
  "dependencies": {
    "com.mochineko.relent-state-machine": "https://github.com/mochi-neko/RelentStateMachine.git?path=/Assets/Mochineko/RelentStateMachine#0.2.0",
    "com.mochineko.relent": "https://github.com/mochi-neko/Relent.git?path=/Assets/Mochineko/Relent#0.2.0",
    "com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask",
    ...
  }
}
```

## Changelog

See [CHANGELOG](https://github.com/mochi-neko/RelentStateMachine/blob/main/CHANGELOG.md).

## 3rd Party Notices

See [NOTICE](https://github.com/mochi-neko/RelentStateMachine/blob/main/NOTICE.md).

## License

Licensed under the [MIT](https://github.com/mochi-neko/RelentStateMachine/blob/main/LICENSE) license.
