# Async Teleport

Teleports you from Sync to Async world. This lib is designed to your apps async from the very Main method.
You can specify terminaators such as key press or SigTerm (usefu lfor services on aws etc.)

## Project Status
**Master** 
[![Build Status](https://travis-ci.org/pamidur/AsyncTeleport.svg?branch=master)](https://travis-ci.org/pamidur/AsyncTeleport)
[![NuGet Release](https://img.shields.io/nuget/vpre/AsyncTeleport.svg)](https://www.nuget.org/packages/AsyncTeleport)

## HowTo

#### Install
```
dotnet add package AsyncTeleport
```

#### Use
```csharp
static async Task MainAsync(string[] args, CancellationToken ct)
{
    await new Program().Run();
    await ct.Wait();
}

static void Main(string[] args)
{
    AsyncTeleport.New()
        .CancelOn(PressKey)
        .CancelOnSigTerm(OnSigTerm)
        .Run(MainAsync, args);
}

private static void OnSigTerm()
{
    Debug.WriteLine("Oh no, SigTerm!");
}

private static void PressKey()
{
    Console.WriteLine("Press Enter to exit.");
    Console.ReadLine();
    Console.WriteLine("Enter is press. Exiting..");
}
```
