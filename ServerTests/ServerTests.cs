﻿using Rust;
using Fougerite;
using Fougerite.Events;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ServerTests
{
    public class ServerTests : Fougerite.Module
    {
        public override string Name
        {
            get { return "ServerTests"; }
        }
        public override string Author
        {
            get { return "Riketta"; }
        }
        public override string Description
        {
            get { return "Testing all hooks and events"; }
        }
        public override Version Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

        Fougerite.Player TesterPlayer = null;

        public override void Initialize()
        {
            Fougerite.Hooks.OnEntityDecay += new Fougerite.Hooks.EntityDecayDelegate(EntityDecay);
            Fougerite.Hooks.OnDoorUse += new Fougerite.Hooks.DoorOpenHandlerDelegate(DoorUse);
            Fougerite.Hooks.OnEntityHurt += new Fougerite.Hooks.EntityHurtDelegate(EntityHurt);
            Fougerite.Hooks.OnPlayerConnected += new Fougerite.Hooks.ConnectionHandlerDelegate(PlayerConnect);
            Fougerite.Hooks.OnPlayerDisconnected += new Fougerite.Hooks.DisconnectionHandlerDelegate(PlayerDisconnect);
            Fougerite.Hooks.OnPlayerHurt += new Fougerite.Hooks.HurtHandlerDelegate(PlayerHurt);
            Fougerite.Hooks.OnPlayerKilled += new Fougerite.Hooks.KillHandlerDelegate(PlayerKilled);
            Fougerite.Hooks.OnServerShutdown += new Fougerite.Hooks.ServerShutdownDelegate(ServerShutdown);
            Fougerite.Hooks.OnShowTalker += new Fougerite.Hooks.ShowTalkerDelegate(ShowTalker);
            Fougerite.Hooks.OnChat += new Fougerite.Hooks.ChatHandlerDelegate(Chat);
            Fougerite.Hooks.OnChatReceived += new Fougerite.Hooks.ChatRecivedDelegate(ChatReceived);
            Fougerite.Hooks.OnCommand += new Fougerite.Hooks.CommandHandlerDelegate(Command);
        }

        public override void DeInitialize()
        {
            Fougerite.Hooks.OnEntityDecay -= new Fougerite.Hooks.EntityDecayDelegate(EntityDecay);
            Fougerite.Hooks.OnDoorUse -= new Fougerite.Hooks.DoorOpenHandlerDelegate(DoorUse);
            Fougerite.Hooks.OnEntityHurt -= new Fougerite.Hooks.EntityHurtDelegate(EntityHurt);
            Fougerite.Hooks.OnPlayerConnected -= new Fougerite.Hooks.ConnectionHandlerDelegate(PlayerConnect);
            Fougerite.Hooks.OnPlayerDisconnected -= new Fougerite.Hooks.DisconnectionHandlerDelegate(PlayerDisconnect);
            Fougerite.Hooks.OnPlayerHurt -= new Fougerite.Hooks.HurtHandlerDelegate(PlayerHurt);
            Fougerite.Hooks.OnPlayerKilled -= new Fougerite.Hooks.KillHandlerDelegate(PlayerKilled);
            Fougerite.Hooks.OnServerShutdown -= new Fougerite.Hooks.ServerShutdownDelegate(ServerShutdown);
            Fougerite.Hooks.OnShowTalker -= new Fougerite.Hooks.ShowTalkerDelegate(ShowTalker);
            Fougerite.Hooks.OnChat -= new Fougerite.Hooks.ChatHandlerDelegate(Chat);
            Fougerite.Hooks.OnChatReceived -= new Fougerite.Hooks.ChatRecivedDelegate(ChatReceived);
            Fougerite.Hooks.OnCommand -= new Fougerite.Hooks.CommandHandlerDelegate(Command);
        }
        
        void Command(Fougerite.Player player, string cmd, string[] args)
        {
            if (cmd == "test" && args.Length == 1)
                TesterPlayer = player;

                switch (args[0])
                {
                    case "all":
                        TestAll();
                        break;
                    default:
                        player.Message("Enter valid arg!");
                        break;
                }
        }

        void TestAll()
        {
            Random Rand = new Random();
            for (int i = 0; i < 5; i++)
            {
                World.GetWorld().Spawn(";deploy_wood_storage_large", TesterPlayer.X + Rand.Next(-10, 10), TesterPlayer.Y, TesterPlayer.Z + Rand.Next(-10, 10));
                World.GetWorld().Spawn(";deploy_largewoodspikewall", TesterPlayer.X + Rand.Next(-10, 10), TesterPlayer.Y, TesterPlayer.Z + Rand.Next(-10, 10));
                World.GetWorld().Spawn(";struct_wood_foundation", TesterPlayer.X + Rand.Next(-10, 10), TesterPlayer.Y, TesterPlayer.Z + Rand.Next(-10, 10));
            }
            Log("Entities placed");
        }

        void Log(string MSG)
        {
            Logger.LogDebug("[Test] " + MSG);
        }

        //

        void BlueprintUse_Test()
        {
            BlueprintDataBlock BDB = new BlueprintDataBlock();
            Log("BlueprintUse_Test: Test 1");
            Hooks.BlueprintUse(null, BDB);

            Log("BlueprintUse_Test: Test 2");
            Hooks.BlueprintUse(null, null);
        }

        void ChatReceived_Test()
        {
            ConsoleSystem.Arg arg = new ConsoleSystem.Arg("");
            Log("ChatReceived_Test: Test 1");
            Hooks.ChatReceived(ref arg);

            arg = null;
            Log("ChatReceived_Test: Test 2");
            Hooks.ChatReceived(ref arg);

            arg = new ConsoleSystem.Arg("say test");
            Log("ChatReceived_Test: Test 3");
            Hooks.ChatReceived(ref arg);
        }

        void ConsoleReceived_Test()
        {
            ConsoleSystem.Arg arg = new ConsoleSystem.Arg("");
            Log("ConsoleReceived_Test: Test 1");
            Hooks.ConsoleReceived(ref arg);

            arg = null;
            Log("ConsoleReceived_Test: Test 2");
            Hooks.ConsoleReceived(ref arg);

            arg = new ConsoleSystem.Arg("say test");
            Log("ConsoleReceived_Test: Test 3");
            Hooks.ConsoleReceived(ref arg);
        }

        void EntityDecay_Test()
        {
            Entity Ent = null;
            if (World.GetWorld().Entities.ToArray().Length > 0)
                Ent = World.GetWorld().Entities.ToArray()[0];
            if (Ent == null)
                Log("EntityDecay_Test: Ent == null!");

            Log("EntityDecay_Test: Test 1");
            Hooks.EntityDecay(Ent, 0f);

            Log("EntityDecay_Test: Test 2");
            Hooks.EntityDecay(Ent, -100f);

            Log("EntityDecay_Test: Test 3");
            Hooks.EntityDecay(Ent, 10000f);

            Log("EntityDecay_Test: Test 4");
            Hooks.EntityDecay(null, 0f);

            Log("EntityDecay_Test: Test 5");
            Hooks.EntityDecay(null, 10000f);

            Ent = new Entity(new object());

            Log("EntityDecay_Test: Test 6");
            Hooks.EntityDecay(Ent, 0f);

            Log("EntityDecay_Test: Test 7");
            Hooks.EntityDecay(Ent, 10000f);
        }

        void EntityDeployed_Test()
        {
            Entity Ent = null;
            if (World.GetWorld().Entities.ToArray().Length > 1)
                Ent = World.GetWorld().Entities.ToArray()[1];
            if (Ent == null)
                Log("EntityDeployed_Test: Ent == null!");

            Log("EntityDeployed_Test: Test 1");
            Hooks.EntityDeployed(Ent);

            Fougerite.Player player = null;
            Ent.ChangeOwner(player);
            Log("EntityDeployed_Test: Test 2");
            Hooks.EntityDeployed(Ent);

            Log("EntityDeployed_Test: Test 3");
            Hooks.EntityDeployed(null);

            Ent = new Entity(new object());
            Log("EntityDeployed_Test: Test 4");
            Hooks.EntityDeployed(Ent);
        }

        void EntityHurt_Test()
        {
            Entity Ent = null;
            if (World.GetWorld().Entities.ToArray().Length > 2)
                Ent = World.GetWorld().Entities.ToArray()[2];
            if (Ent == null)
                Log("EntityHurt_Test: Ent == null!");

            DamageEvent damageEvent = new DamageEvent();
            damageEvent.amount = 50f;

            Log("EntityHurt_Test: Test 1");
            Hooks.EntityHurt(Ent, ref damageEvent);

            Fougerite.Player player = null;
            Ent.ChangeOwner(player);
            Log("EntityHurt_Test: Test 2");
            Hooks.EntityHurt(Ent, ref damageEvent);

            Log("EntityHurt_Test: Test 3");
            Hooks.EntityHurt(null, ref damageEvent);

            Ent = new Entity(new object());
            Log("EntityHurt_Test: Test 4");
            Hooks.EntityHurt(Ent, ref damageEvent);
        }

        void NPCHurt_Test()
        {
            DamageEvent damageEvent = new DamageEvent();
            Log("NPCHurt_Test: Test 1");
            Hooks.NPCHurt(ref damageEvent);
        }

        void NPCKilled_Test()
        {
            DamageEvent damageEvent = new DamageEvent();
            Log("NPCKilled_Test: Test 1");
            Hooks.NPCKilled(ref damageEvent);
        }

        void PlayerConnect_Test()
        {
            NetUser NUser = null;
            Log("PlayerConnect_Test: Test 1");
            Hooks.PlayerConnect(NUser);

            Log("PlayerConnect_Test: Test 2");
            Hooks.PlayerConnect(TesterPlayer.PlayerClient.netUser);
        }

        void PlayerDisconnect_Test()
        {
            NetUser NUser = null;
            Log("PlayerConnect_Test: Test 1");
            Hooks.PlayerDisconnect(NUser);

            Log("PlayerConnect_Test: Test 2");
            Hooks.PlayerDisconnect(TesterPlayer.PlayerClient.netUser);
        }

        //

        void ChatReceived(ref ConsoleSystem.Arg arg)
        {
        }

        void Chat(Fougerite.Player p, ref ChatString text)
        {
        }

        void ShowTalker(uLink.NetworkPlayer player, PlayerClient p)
        {
        }

        void ServerShutdown()
        {
        }

        void PlayerKilled(DeathEvent deathEvent)
        {
        }

        void EntityDecay(DecayEvent de)
        {
        }

        void DoorUse(Fougerite.Player p, DoorEvent de)
        {
        }

        void PlayerHurt(HurtEvent he)
        {
        }

        void PlayerDisconnect(Fougerite.Player player)
        {
        }

        void PlayerConnect(Fougerite.Player player)
        {
        }

        void EntityHurt(HurtEvent he)
        {
        }
    }
}
