# Async Teleport

Teleports you from Sync to Async world. This lib is designed to make your apps async from the very Main method.
You can specify terminators such as _key press_, _CTRL+C_ or _SigTerm_ (useful for services on aws etc.)

## Project Status
[![Build Status](https://travis-ci.org/pamidur/AsyncTeleport.svg?branch=master)](https://travis-ci.org/pamidur/AsyncTeleport)
[![NuGet Release](https://img.shields.io/nuget/vpre/AsyncTeleport.svg)](https://www.nuget.org/packages/AsyncTeleport)

## HowTo

#### Install
```
dotnet add package AsyncTeleport
```

#### Use
```csharp
static void Main(string[] args)
{
    AsyncTeleport.New()
        .CancelOn(PressKey)
        .CancelOnGracefulShutdown(OnGracefulShutdown)
        .Run(MainAsync, args);
}

static async Task MainAsync(string[] args, CancellationToken ct)
{
    await new Program().Run();
    await ct.Wait();
}

private static void OnGracefulShutdown()
{
    Debug.WriteLine("Oh no, SigTerm or CTRL+C !");
}

private static void PressKey()
{
    Console.WriteLine("Press Enter to exit.");
    Console.ReadLine();
    Console.WriteLine("Enter is press. Exiting..");
}
```
