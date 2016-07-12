using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Text;

namespace laftho.LaserComm
{
    class Program : Sandbox.ModAPI.Ingame.MyGridProgram
    {

        #region In-Game Script

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
        
        public class LaserComm
        {
            /* Laser Antenna Cross-Grid Data Communication Script for Space Engineers
               Thomas LaFreniere aka laftho
               v1.0 - July 12, 2016
           */
            string[] cruf = new string[] { "Rotating towards ", "Trying to establish connection to ", "Connected to " };

            Dictionary<string, string> translation = new Dictionary<string, string>()
            {
                { "newMessage", "New Message: {0}" },
                { "laserNotFound", "Unable to find laser antenna by tag: {0}" }
            };

            string laserTag;
            IMyLaserAntenna laser;
            Action<string> log;
            
            public LaserComm(IMyGridTerminalSystem sys, string laserTag, Action<string> log = null)
            {
                IMyLaserAntenna laserBlock = null;
                this.laserTag = laserTag;

                var blocks = new List<IMyTerminalBlock>();

                sys.GetBlocksOfType<IMyLaserAntenna>(blocks, block => block.CustomName.Contains(laserTag));

                if (blocks.Count > 0) laserBlock = (IMyLaserAntenna)blocks[0];

                if (laserBlock == null) throw new Exception(string.Format(translation["laserNotFound"], laserTag));
                
                blocks = null;

                init(laserBlock, log);
            }

            public LaserComm(IMyLaserAntenna laser, Action<string> log = null)
            {
                init(laser, log);
            }

            private void init(IMyLaserAntenna laser, Action<string> log = null)
            {
                if (string.IsNullOrEmpty(laserTag))
                    laserTag = laser.CustomName;

                this.laser = laser;
                this.log = log;
            }

            private void output(string message)
            {
                if (log != null)
                    log.Invoke(message);
            }
            
            string val = string.Empty;

            public void Tick()
            {
                laser.ApplyAction("OnOff_Off");
                laser.ApplyAction("OnOff_On");

                var info = laser.DetailedInfo.Split('\n');

                if (info.Length != 3) return;

                var message = info[2];

                foreach (var c in cruf)
                {
                    if (message.StartsWith(c))
                        message = message.Substring(c.Length);
                }

                if (message.Contains("[msg:"))
                {
                    int start = message.LastIndexOf("[msg:");

                    if (start < 0) return;

                    start += 5;

                    int end = message.LastIndexOf("]");

                    if (end < 0) return;

                    end = end - start;

                    if (end < 0) return;

                    message = message.Substring(start, end);

                    if (message != val)
                    {
                        val = message;
                        output(string.Format(translation["newMessage"], val));
                        process(val);
                    }
                }
            }

            private void process(string val)
            {
                foreach(var set in registry)
                {
                    if (set.Matches(val))
                        set.Act(val);
                }
            }
            
            public class LaserCommActionSet
            {
                Predicate<string> match;
                Action<string> action;

                public bool Matches(string val)
                {
                    if (match != null)
                        return match.Invoke(val);

                    return false;
                }

                public void Act(string val)
                {
                    action.Invoke(val);
                }

                public LaserCommActionSet(Predicate<string> match, Action<string> action)
                {
                    this.match = match;
                    this.action = action;
                }
            }

            List<LaserCommActionSet> registry = new List<LaserCommActionSet>();

            public void On(string message, Action<string> action)
            {
                On(new Predicate<string>(msg => msg == message), action);
            }

            public void On(Predicate<string> match, Action<string> action)
            {
                On(new LaserCommActionSet(match, action));
            }

            public void On(LaserCommActionSet set)
            {
                registry.Add(set);
            }

            public void Send(string message)
            {
                laser.SetCustomName(string.Format("{0} [msg:{1}]", laserTag, message));
            }
        }


        #endregion
    }
}
