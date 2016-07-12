Space Engineers Laser Antenna Communication In-Game Script

Full duplex point-to-point cross grid data communication via laser antennas. This library is high latency, ~2sec, so it is best suited for high level commands or information relay.

Use with linked deep space probes for early intrusion detection. Coordinate relay link deployment or drone fleet formations. Operation command communication layer for coordination with either complex military or industrial operations.

Usage:

Declare a program level variable for LaserComm, initialize once and register any on command predicate and action sets. This is ensures the script runs efficiently and reliably. ie.

```
LaserComm comm;

public void Main(string arguments)
{
    ...
```


Instanciate LaserComm to the global variable only if it is null. This ensures it'll only create a single instance when the game starts. Here is a good place to register your On events too.
```
    if (comm == null)
    {
        comm = new LaserComm(GridTerminalSystem, "[tx]", (message) => { Echo(message + "\n"); });

        comm.On...
    }
```

The LaserComm constructor takes up to three arguments:

1. GridTerminalSystem
2. string for a partial match (tag) on the laser antenna name. This does laserAntenna.CustomName.Contains(value). Ensure these are unique across all laser antennas on your grid.
3. optional logging action. A simple Echo(message) will put the log to the Programmable Block detailed info. A good recommendation is to log to an LCD or Text Panel. This simply executes with message whenever a new message is received.

Overloads for LaserComm constructor allow you to put in an instance of IMyLaserAntenna but there may be a risk of command word collisons so this is not recommend unless you have a clean concise name that does not include any reserved words, namely [msg:]

When you're first instanciating LaserComm it is highly recommended to also add all of your On registers at this time. If you have conditional On registers during run-time, just be sure you only register them once per instance of LaserComm, otherwise code will execute once for each time you register it.
```
    if (comm == null)
    {
        comm = new LaserComm(GridTerminalSystem, "[tx]", (message) => { Echo(message + "\n"); });

        comm.On("ping", new Action<string>((message) =>
        {
            comm.Send("pong");
        }));

        comm.On...
    }
```

`comm.On` can register either an exact string match for the incoming message or you can provide a string predicate to write whatever code that you want to process and match the message.

```
comm.On(
    new Predicate<string>((message) => message.Contains("some part")),  // string predicate or exact string
    (message) => { ... }                                                // action to execute if the predicate/string matches
);
```

Putting it all together:

```
LaserComm comm;

public void Main(string arguments)
{
    if (comm == null)
    {
        comm = new LaserComm(GridTerminalSystem, "[tx]", (message) => { Echo(message + "\n"); });

        comm.On("ping", new Action<string>((message) =>
        {
            comm.Send("pong");
        }));

        comm.On("pong", new Action<string>((message) =>
        {
            comm.Send("ping");
        }));
    }
            
    comm.Tick();
}

public class LaserComm {...}
```

The above will look for a Laser Antenna with a name that at least includes [tx] and log messages to Echo. If it receives a ping message it will send pong message, if it receives pong it will send ping. You could setup two grids with this example and on one of the grids, simply change the script to send ping on ping and send pong on pong. The two grids will then indefinitely send ping & pong back and forth.

When you want the laser antenna to actually process the potential messages, comm.Tick(). Tick() should execute exactly once per script run.

Script should be attached to a 1s timer loop. Any less than 1s and it executes do quickly to properly allow time for the laser antennas to refresh their connections and receive the latest messages. A future version could look at finer tick timers to ensure reliablity and reduce latency.


