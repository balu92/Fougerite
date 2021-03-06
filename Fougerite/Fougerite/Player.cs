﻿using System.Diagnostics.Contracts;

namespace Fougerite
{
    using Facepunch.Utility;
    using Fougerite.Events;
    using Rust;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using uLink;
    using UnityEngine;

    public class Player
    {
        private readonly PlayerClient ourPlayer;
        private readonly long connectedAt;

        private PlayerInv inv;
        private bool invError;
        private bool justDied;
        

        //public Player()
        //{
        //    this.justDied = true;
        //}

        public Player(PlayerClient client)
        {
            Contract.Requires(client != null);

            this.justDied = true;
            this.ourPlayer = client;
            this.connectedAt = DateTime.UtcNow.Ticks;
            this.FixInventoryRef();
        }

        [ContractInvariantMethod]
        private void Invariant()
        {
            Contract.Invariant(ourPlayer != null);
        }

        public void Disconnect()
        {
            NetUser netUser = this.ourPlayer.netUser;

            if (netUser == null)
                throw new InvalidOperationException("Player's netUser is null.");

            if (netUser.connected)
                netUser.Kick(NetError.NoError, true);
        }

        public Fougerite.Player Find(string search)
        {
            Contract.Requires(!string.IsNullOrEmpty(search));

            Fougerite.Player player = FindBySteamID(search);
            if (player != null)
            {
                return player;
            }
            player = FindByGameID(search);
            if (player != null)
            {
                return player;
            }
            player = FindByName(search);
            if (player != null)
            {
                return player;
            }
            return null;
        }

        public static Fougerite.Player FindByGameID(string uid)
        {
            Contract.Requires(!string.IsNullOrEmpty(uid));

            foreach (Fougerite.Player player in Fougerite.Server.GetServer().Players)
                if (player != null && player.GameID == uid)
                    return player;
            return null;
        }

        public static Fougerite.Player FindByName(string name)
        {
            Contract.Requires(!string.IsNullOrEmpty(name));

            foreach (Fougerite.Player player in Fougerite.Server.GetServer().Players)
                if (player != null && player.Name.ToLower() == name.ToLower())
                    return player;
            return null;
        }

        public static Fougerite.Player FindByNetworkPlayer(uLink.NetworkPlayer np)
        {
            if (np == null) return null;

            foreach (Fougerite.Player player in Fougerite.Server.GetServer().Players)
                if (player != null && player.ourPlayer.netPlayer == np)
                    return player;
            return null;
        }

        public static Fougerite.Player FindByPlayerClient(PlayerClient pc)
        {
            if (pc == null) return null;

            foreach (Fougerite.Player player in Fougerite.Server.GetServer().Players)
                if (player!= null && player.PlayerClient == pc)
                    return player;
            return null;
        }

        public static Fougerite.Player FindBySteamID(string uid)
        {
            Contract.Requires(!string.IsNullOrEmpty(uid));

            foreach (Fougerite.Player player in Fougerite.Server.GetServer().Players)
                if (player != null && player.SteamID == uid)
                    return player;
            return null;
        }

        public void FixInventoryRef()
        {
            Hooks.OnPlayerKilled += new Hooks.KillHandlerDelegate(this.Hooks_OnPlayerKilled);
        }
		
	public bool HasBlueprint(BlueprintDataBlock dataBlock)
        {
            PlayerInventory invent = this.Inventory.InternalInventory as PlayerInventory;
            if (invent.KnowsBP(dataBlock))
                return true;
            return false;
        }

        private void Hooks_OnPlayerKilled(DeathEvent de)
        {
            Contract.Requires(de != null);

            try
            {
                Fougerite.Player victim = de.Victim as Fougerite.Player;
                if (victim.GameID == this.GameID)
                {
                    this.justDied = true;
                }
            }
            catch (Exception ex)
            {
                this.invError = true;
                Logger.LogException(ex);
            }
        }

        public void InventoryNotice(string arg)
        {
            Contract.Requires(arg != null);

            Rust.Notice.Inventory(this.ourPlayer.netPlayer, arg);
        }

        public void Kill()
        {
            TakeDamage.KillSelf(this.PlayerClient.controllable.character, null);
        }

        public void Message(string arg)
        {
            Contract.Requires(arg != null);

            this.SendCommand("chat.add " + Facepunch.Utility.String.QuoteSafe(Fougerite.Server.GetServer().server_message_name) + " " + Facepunch.Utility.String.QuoteSafe(arg));
        }

        public void MessageFrom(string playername, string arg)
        {
            Contract.Requires(!string.IsNullOrEmpty(playername));
            Contract.Requires(arg != null);

            this.SendCommand("chat.add " + Facepunch.Utility.String.QuoteSafe(playername) + " " + Facepunch.Utility.String.QuoteSafe(arg));
        }

        public void Notice(string arg)
        {
            Contract.Requires(arg != null);

            Rust.Notice.Popup(this.PlayerClient.netPlayer, "!", arg, 4f);
        }

        public void Notice(string icon, string text, float duration = 4f)
        {
            Contract.Requires(icon != null);
            Contract.Requires(text != null);
            Contract.Requires(duration >= 0);

            Rust.Notice.Popup(this.PlayerClient.netPlayer, icon, text, duration);
        }

        public void SendCommand(string cmd)
        {
            Contract.Requires(cmd != null);

            ConsoleNetworker.SendClientCommand(this.PlayerClient.netPlayer, cmd);
        }

        public void TeleportToAndFace(Fougerite.Player p, float distance = 1.5f)
        { 
            Contract.Requires(p.PlayerClient.controllable.transform != null);

            if (this == p) // lol
                return;

            Transform transform = p.PlayerClient.controllable.transform;                                            // get the target player's transform
            Vector3 target = transform.TransformPoint(new Vector3(0f, 0f, (this.Admin ? -distance : distance)));    // rcon admin teleports behind target player
            this.TeleportTo(target);
            this.ourPlayer.controllable.transform.LookAt(transform);                                                // turn towards the target player's transform
        }

        public void SafeTeleportTo(float x, float y, float z)
        {
            this.SafeTeleportTo(new Vector3(x, y, z));
        }

        public void SafeTeleportTo(float x, float z)
        {
            this.SafeTeleportTo(new Vector3(x, 0f, z));
        }

        public void SafeTeleportTo(Vector3 target)
        {
            float height = Terrain.activeTerrain.SampleHeight(target);
            IEnumerable<StructureMaster> structures = from s in StructureMaster.AllStructures
                                                where s.containedBounds.Contains(target)
                                                select s;
            if (height > target.y)
                target.y = height;

            if (structures.Count() >= 1) {
                this.TeleportTo(target);
                System.Threading.Thread.Sleep(800);
                this.TeleportTo(target.x, target.y, target.z);
                return;
            }
            this.TeleportTo(target);
        }

        public void TeleportTo(float x, float y, float z)
        {
            this.TeleportTo(new Vector3(x, y, z));
        }

        public void TeleportTo(Fougerite.Player p)
        {
            Contract.Requires(p.Location != null);

            if (this == p) // lol
                return;
                
            this.TeleportTo(p.Location);
        }

        public void TeleportTo(Vector3 target)
        {
            RustServerManagement.Get().TeleportPlayerToWorld(this.PlayerClient.netPlayer, target);
        }

        public bool Admin
        {
            get
            {
                return this.ourPlayer.netUser.admin;
            }
        }

        public string GameID
        {
            get
            {
                return this.ourPlayer.userID.ToString();
            }
        }

        public float Health
        {
            get
            {
                return this.PlayerClient.controllable.health;
            }
            set
            {
                this.PlayerClient.controllable.takeDamage.health = value;
                this.PlayerClient.controllable.takeDamage.Heal(this.PlayerClient.controllable, 0f);
            }
        }

        public PlayerInv Inventory
        {
            get
            {
                if (this.invError || this.justDied)
                {
                    this.inv = new PlayerInv(this);
                    this.invError = false;
                    this.justDied = false;
                }
                return this.inv;
            }
        }

        public string IP
        {
            get
            {
                return this.ourPlayer.netPlayer.externalIP;
            }
        }

        public bool IsBleeding
        {
            get
            {
                return this.PlayerClient.controllable.GetComponent<HumanBodyTakeDamage>().IsBleeding();
            }
            set
            {
                this.PlayerClient.controllable.GetComponent<HumanBodyTakeDamage>().SetBleedingLevel((float)Convert.ToInt32(value));
            }
        }

        public bool IsCold
        {
            get
            {
                return this.PlayerClient.controllable.GetComponent<Metabolism>().IsCold();
            }
            set
            {
                this.PlayerClient.controllable.GetComponent<Metabolism>().coreTemperature = value ? ((float)(-10)) : ((float)10);
            }
        }

        public bool IsInjured
        {
            get
            {
                return (this.PlayerClient.controllable.GetComponent<FallDamage>().GetLegInjury() > 0);
            }
            set
            {
                this.PlayerClient.controllable.GetComponent<FallDamage>().SetLegInjury((float)Convert.ToInt32(value));
            }
        }

        public Vector3 Location
        {
            get
            {
                return this.ourPlayer.lastKnownPosition;
            }
            set
            {
                this.ourPlayer.transform.position.Set(value.x, value.y, value.z);
            }
        }

        public string Name
        {
            get
            {
                return this.ourPlayer.netUser.user.displayname_; // displayname_
            }
            set
            {
                this.ourPlayer.netUser.user.displayname_ = value; // displayname_
                this.ourPlayer.userName = this.ourPlayer.netUser.user.displayname_; // displayname_
            }
        }

        public int Ping
        {
            get
            {
                return this.ourPlayer.netPlayer.averagePing;
            }
        }

        public PlayerClient PlayerClient
        {
            get
            {
                return this.ourPlayer;
            }
        }

        public string SteamID
        {
            get
            {
                return this.ourPlayer.netUser.userID.ToString();
            }
        }

        public long TimeOnline
        {
            get
            {
                return ((DateTime.UtcNow.Ticks - this.connectedAt) / 0x2710L);
            }
        }

        public float X
        {
            get
            {
                return this.ourPlayer.lastKnownPosition.x;
            }
            set
            {
                this.ourPlayer.transform.position.Set(value, this.Y, this.Z);
            }
        }

        public float Y
        {
            get
            {
                return this.ourPlayer.lastKnownPosition.y;
            }
            set
            {
                this.ourPlayer.transform.position.Set(this.X, value, this.Z);
            }
        }

        public float Z
        {
            get
            {
                return this.ourPlayer.lastKnownPosition.z;
            }
            set
            {
                this.ourPlayer.transform.position.Set(this.X, this.Y, value);
            }
        }
    }
}
