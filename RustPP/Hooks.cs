﻿namespace RustPP
{
    using Fougerite;
    using RustPP.Commands;
    using RustPP.Permissions;
    using RustPP.Social;
    using System;
    using System.Collections;

    internal class Hooks
    {
        public static void broadcastDeath(string victim, string killer, string weapon)
        {
            try
            {
                if (Core.config.GetSetting("Settings", "pvp_death_broadcast").ToLower() == "true")
                {
                    char ch = '⊕';
                    Util.sayAll(Core.Name, killer + " " + ch.ToString() + " " + victim + " (" + weapon + ")");
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public static bool checkOwner(DeployableObject obj, Controllable controllable)
        {
            bool flag;
            if (obj.ownerID == controllable.playerClient.userID)
            {
                return true;
            }
            try
            {
                SleepingBag bag1 = (SleepingBag)obj;
                flag = false;
            }
            catch
            {
                try
                {
                    ShareCommand command = ChatCommand.GetCommand("share") as ShareCommand;
                    ArrayList list = (ArrayList)command.GetSharedDoors()[obj.ownerID];
                    if (list == null)
                    {
                        return false;
                    }
                    if (list.Contains(controllable.playerClient.userID))
                    {
                        return true;
                    }
                    flag = false;
                }
                catch (Exception)
                {
                    flag = false;
                }
            }
            return flag;
        }

        public static bool decayDisabled()
        {
            try
            {
                return (Core.config.GetSetting("Settings", "decay").ToLower() == "false");
            }
            catch
            {
                return false;
            }
        }

        public static void deployableKO(DeployableObject dep, DamageEvent e)
        {
            try
            {
                InstaKOCommand command = ChatCommand.GetCommand("instako") as InstaKOCommand;
                if (command.IsOn(e.attacker.client.userID))
                {
                    try
                    {
                        Helper.Log("StructDestroyed.txt", string.Concat(new object[] { e.attacker.client.netUser.displayName, " [", e.attacker.client.netUser.userID, "] destroyed (InstaKO) ", NetUser.FindByUserID(dep.ownerID).displayName, "'s ", dep.gameObject.name.Replace("(Clone)", "") }));
                    }
                    catch
                    {
                        if (Core.userCache.ContainsKey(dep.ownerID))
                        {
                            Helper.Log("StructDestroyed.txt", string.Concat(new object[] { e.attacker.client.netUser.displayName, " [", e.attacker.client.netUser.userID, "] destroyed (InstaKO) ", Core.userCache[dep.ownerID], "'s ", dep.gameObject.name.Replace("(Clone)", "") }));
                        }
                    }
                    dep.OnKilled();
                }
                else
                {
                    dep.UpdateClientHealth();
                }
            }
            catch
            {
                dep.UpdateClientHealth();
            }
        }

        public static bool IsFriend(DamageEvent e) // ref
        {
            GodModeCommand command = (GodModeCommand)ChatCommand.GetCommand("god");
            try
            {
                FriendsCommand command2 = (FriendsCommand)ChatCommand.GetCommand("friends");
                FriendList list = (FriendList)command2.GetFriendsLists()[e.attacker.userID];
                if (Core.config.GetSetting("Settings", "friendly_fire").ToLower() == "true")
                {
                    return command.IsOn(e.victim.userID);
                }
                if (list == null)
                {
                    return command.IsOn(e.victim.userID);
                }
                return (list.isFriendWith(e.victim.userID) || command.IsOn(e.victim.userID));
            }
            catch
            {
                return command.IsOn(e.victim.userID);
            }
        }

        public static bool KeepItem()
        {
            try
            {
                string keepitems = Core.config.GetSetting("Settings", "keepitems").ToLower();
                return keepitems == "true";
            }
            catch
            {
                return false;
            }
        }

        public static bool loginNotice(NetUser user)
        {
            try
            {
                if (Core.blackList.Contains(user.userID))
                {
                    Core.tempConnect.Add(user.userID);
                    user.Kick(NetError.Facepunch_Connector_VAC_Banned, true);
                    return false;
                }
                if (((Core.config.GetSetting("WhiteList", "enabled") != null) && (Core.config.GetSetting("WhiteList", "enabled").ToLower() == "true")) && !Core.whiteList.Contains(user.userID))
                {
                    user.Kick(NetError.Facepunch_Connector_AuthFailure, true);
                }
                if (!Core.userCache.ContainsKey(user.userID))
                {
                    Core.userCache.Add(user.userID, user.displayName);
                }
                else if (user.displayName != Core.userCache[user.userID])
                {
                    Core.userCache[user.userID] = user.displayName;
                }
                if (Administrator.IsAdmin(user.userID) && Administrator.GetAdmin(user.userID).HasPermission("RCON"))
                {
                    user.admin = true;
                }
                Core.motd(user.networkPlayer);
                if (Core.config.GetSetting("Settings", "join_notice").ToLower() == "true")
                {
                    foreach (PlayerClient client in PlayerClient.All)
                    {
                        if (client.userID != user.userID)
                        {
                            Util.sayUser(client.netPlayer, Core.Name, user.displayName + " has joined the server");
                        }
                    }
                }
            }
            catch
            {
            }
            return true;
        }

        public static void logoffNotice(NetUser user)
        {
            try
            {
                if (Core.tempConnect.Contains(user.userID))
                {
                    Core.tempConnect.Remove(user.userID);
                }
                else if (Core.config.GetSetting("Settings", "leave_notice").ToLower() == "true")
                {
                    foreach (PlayerClient client in PlayerClient.All)
                    {
                        if (client.userID != user.userID)
                        {
                            Util.sayUser(client.netPlayer, Core.Name, user.displayName + " has left the server");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public static void structureKO(StructureComponent sc, DamageEvent e)
        {
            try
            {
                InstaKOCommand command = ChatCommand.GetCommand("instako") as InstaKOCommand;
                if (command.IsOn(e.attacker.client.userID))
                {
                    sc.StartCoroutine("DelayedKill");
                }
                else
                {
                    sc.UpdateClientHealth();
                }
            }
            catch
            {
                sc.UpdateClientHealth();
            }
        }
    }
}