using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2;
using RoR2.ConVar;
using RoR2.Networking;
using System.IO;
using System.Reflection;
using System.Globalization;
using UnityEngine.Networking;
using RoR2.UI;

namespace RoRMod
{
    public class Mod
    {
        public static bool isInitialized;

        public static IntConVar mod_engi_max_mines = new IntConVar( "mod_engi_max_mines", ConVarFlags.None, "10", "" );
        public static IntConVar mod_engi_max_turrets = new IntConVar( "mod_engi_max_turrets", ConVarFlags.None, "2", "" );
        public static IntConVar mod_engi_max_bubbles = new IntConVar( "mod_engi_max_bubbles", ConVarFlags.None, "1", "" );
        public static List<CharacterMaster> spawns = new List<CharacterMaster>();

        public static void Init()
        {
            if ( !isInitialized )
                return;
            isInitialized = true;
            Debug.Log( "hi! th is is RoRMod!" );
            Debug.LogWarning( "hi! th is is RoRMod!" );
            Debug.LogError( "hi! th is is RoRMod!" );
        }

        public static void SpawnMasterWithBody( string masterPrefab, string bodyPrefab, Vector3 pos, TeamIndex team, Inventory inv = null )
        {
            GameObject obj = GameObject.Instantiate( MasterCatalog.FindMasterPrefab( masterPrefab ), pos, Quaternion.identity );
            GameObject bodyObj = BodyCatalog.FindBodyPrefab( bodyPrefab );
            CharacterMaster master = obj.GetComponent<CharacterMaster>();
            master.teamIndex = team;

            if ( inv != null )
                master.inventory.CopyItemsFrom( inv );

            NetworkServer.Spawn( obj );
            master.SpawnBody( bodyObj, pos, Quaternion.identity );
            spawns.Add( master );
        }

        [ConCommand( commandName = "mod_set_skill" )]
        public static void CCSetSkill( ConCommandArgs args )
        {
            args.CheckArgumentCount( 2 );
            SkillSlot slot;
            if ( !Enum.TryParse( args[0], true, out slot ) )
            {
                Debug.Log( "Invalid slot. valid ones are" );
                foreach ( string s in Enum.GetNames( typeof( SkillSlot ) ) )
                    Debug.Log( s );
                return;
            }

            if ( slot == SkillSlot.None )
            {
                Debug.Log( "no." );
                return;
            }

            var skillLocator = args.senderMasterObject.GetComponent<CharacterMaster>().GetBody().GetComponent<SkillLocator>();
            var skill = skillLocator.FindSkill( args[1] );
            if ( !skill )
            {
                Debug.Log( "Invalid spell name. execute mod_list_skills to see all." );
                return;
            }
            switch ( slot )
            {
                case SkillSlot.Primary:
                    skillLocator.primary = skill;
                    break;
                case SkillSlot.Secondary:
                    skillLocator.secondary = skill;
                    break;
                case SkillSlot.Special:
                    skillLocator.special = skill;
                    break;
                case SkillSlot.Utility:
                    skillLocator.utility = skill;
                    break;
            }
        }

        public static void PrintComponent( GameObject go )
        {
            foreach ( Component c in go.GetComponents( typeof( Component ) ) )
                Debug.Log( c.name + " (" + c.tag + ", " + c.GetType().Name + ")" );
        }

        [ConCommand( commandName = "mod_list_enum" )]
        public static void CCListEnum( ConCommandArgs args )
        {
            args.CheckArgumentCount( 1 );
            Type t = null;

            foreach ( Assembly a in AppDomain.CurrentDomain.GetAssemblies() )
                if ( ( t = a.GetType( args[0], false, true ) ) != null )
                    break;

            if ( t == null || !t.IsEnum )
            {
                Debug.LogFormat( "Invalid type {0}", args[0] );
                return;
            }

            Debug.LogFormat( "Names and values for Enum {0}", t.Name );
            foreach ( string s in Enum.GetNames( t ) )
                Debug.Log( s );
        }

        [ConCommand( commandName = "mod_list_skills" )]
        public static void CCListSkills( ConCommandArgs args )
        {
            var skillLocator = args.senderMasterObject.GetComponent<CharacterMaster>().GetBody().GetComponent<SkillLocator>();
            if ( !skillLocator )
            {
                Debug.LogFormat( "couldn't get skilllocator for sender ({0} / {1})", args.senderMasterObject, args.sender );
                return;
            }

            foreach ( var skill in skillLocator.GetComponents<GenericSkill>() )
                Debug.LogFormat( "{0} ({1}), type {2}", skill.skillName, skill.skillNameToken, skill.GetType().Name );
        }

        [ConCommand( commandName = "mod_engi_killall" )]
        public static void CCEngiKillall( ConCommandArgs args )
        {
            var master = args.senderMasterObject.GetComponent<CharacterMaster>();
            List<DeployableInfo> deployables = (List<DeployableInfo>)master.GetType().GetField( "deployablesList", BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance ).GetValue( master );
            if ( deployables == null ) // this list can actually be null sometimes
                return;
            Debug.Log( deployables );
            foreach ( DeployableInfo d in deployables )
            {
                if ( d.deployable )
                {
                    d.deployable.ownerMaster = null;
                    d.deployable.onUndeploy.Invoke();
                }
            }
            deployables.Clear();
        }

        [ConCommand( commandName = "mod_spawn_portal_ms" )]
        public static void CCSpawnPortalMS( ConCommandArgs args )
        {
            if ( !TeleporterInteraction.instance )
            {
                Debug.Log( "TeleporterInteraction singleton instance is null." );
                return;
            }
            TeleporterInteraction.instance.shouldAttemptToSpawnMSPortal = true;
        }

        [ConCommand( commandName = "mod_spawn_portal_shop" )]
        public static void CCSpawnPortalShop( ConCommandArgs args )
        {
            if ( !TeleporterInteraction.instance )
            {
                Debug.Log( "TeleporterInteraction singleton instance is null." );
                return;
            }
            TeleporterInteraction.instance.shouldAttemptToSpawnShopPortal = true;
        }

        [ConCommand( commandName = "mod_spawn_portal_shore" )]
        public static void CCSpawnPortalShore( ConCommandArgs args )
        {
            if ( !TeleporterInteraction.instance )
            {
                Debug.Log( "TeleporterInteraction singleton instance is null." );
                return;
            }
            TeleporterInteraction.instance.shouldAttemptToSpawnGoldshoresPortal = true;
        }

        [ConCommand( commandName = "sudo", helpText = "Execute a command as a different user. Must have the ExecuteOnServer flag." )]
        public static void CCSudo( ConCommandArgs args )
        {
            // check args manually
            if ( args.Count <= 1 )
            {
                Debug.Log( "Usage: sudo <partial username/*> <command...>" );
                return;
            }

            if ( args[0] == "*" )
            {
                foreach ( NetworkUser u in NetworkUser.readOnlyInstancesList )
                    typeof( RoR2.Console ).GetMethod( "RunCmd", BindingFlags.NonPublic | BindingFlags.Instance ).Invoke( RoR2.Console.instance, new object[] { u, args[1], args.userArgs.GetRange( 2, args.userArgs.Count - 2 ) } );
                return;
            }

            NetworkUser sudoUser = null;
            foreach ( NetworkUser u in NetworkUser.readOnlyInstancesList )
            {
                if ( u.userName.IndexOf( args[0], StringComparison.CurrentCultureIgnoreCase ) >= 0 )
                {
                    sudoUser = u;
                    break;
                }
            }

            if ( sudoUser == null )
            {
                Debug.Log( "Invalid user." );
                return;
            }

            typeof( RoR2.Console ).GetMethod( "RunCmd", BindingFlags.NonPublic | BindingFlags.Instance ).Invoke( RoR2.Console.instance, new object[] { sudoUser, args[1], args.userArgs.GetRange( 2, args.userArgs.Count - 2 ) } );
        }

        [ConCommand( commandName = "mod_killall" )]
        public static void CCKillAllSpawned( ConCommandArgs args )
        {
            foreach ( CharacterMaster master in spawns )
                if ( master )
                    master.TrueKill();
            spawns.Clear();
        }

        [ConCommand( commandName = "mod_usermod" )]
        public static void CCUserMod( ConCommandArgs args )
        {
            Debug.Log( "do NOT use this with sudo!" );
            args.senderMasterObject.AddComponent<UserModOptions>();
        }

        public static void PrintAllChildren( GameObject go )
        {
            Debug.LogFormat( "{0} ({1})", go.name, go.GetType().Name );
            foreach ( Component c in go.GetComponents<Component>() )
                Debug.LogFormat( "- {0}", c.GetType().Name );
        }

        [ConCommand( commandName = "mod_test" )]
        public static void CCTest( ConCommandArgs args )
        {

        }

        [ConCommand( commandName = "mod_spawn_drone" )]
        public static void CCSpawnDrone( ConCommandArgs args )
        {
            CharacterMaster master = args.senderMasterObject.GetComponent<CharacterMaster>();
            TeamIndex team = master.teamIndex;
            if ( args.Count >= 1 )
                Enum.TryParse( args[0], true, out team );

            SpawnMasterWithBody( "Drone1Master", "Drone1Body", master.GetBody().transform.position, team, master.inventory );
        }

        [ConCommand( commandName = "mod_spawn_drone_heal" )]
        public static void CCSpawnDroneHeal( ConCommandArgs args )
        {
            CharacterMaster master = args.senderMasterObject.GetComponent<CharacterMaster>();
            TeamIndex team = master.teamIndex;
            if ( args.Count >= 1 )
                Enum.TryParse( args[0], true, out team );

            SpawnMasterWithBody( "Drone2Master", "Drone2Body", master.GetBody().transform.position, team, master.inventory );
        }

        [ConCommand( commandName = "mod_spawn_turret" )]
        public static void CCSpawnTurret( ConCommandArgs args )
        {
            CharacterMaster master = args.senderMasterObject.GetComponent<CharacterMaster>();
            TeamIndex team = master.teamIndex;
            if ( args.Count >= 1 )
                Enum.TryParse( args[0], true, out team );

            SpawnMasterWithBody( "EngiTurretMaster", "EngiTurretBody", master.GetBody().transform.position, team, master.inventory );
        }

        [ConCommand( commandName = "mod_spawn_give" )]
        public static void CCGiveSpawnedItem( ConCommandArgs args )
        {
            args.CheckArgumentCount( 1 );
            ItemIndex item;
            int count = 1;

            if ( !Enum.TryParse( args[0], true, out item ) )
                return;
            if ( args.Count >= 2 )
                int.TryParse( args[1], out count );

            foreach ( CharacterMaster master in spawns )
                if ( master )
                    master.inventory.GiveItem( item, count );
        }

        [ConCommand( commandName = "mod_spawn" )]
        public static void CCSpawnGeneral( ConCommandArgs args )
        {
            args.CheckArgumentCount( 2 );
            CharacterMaster master = args.senderMasterObject.GetComponent<CharacterMaster>();
            TeamIndex team = master.teamIndex;
            if ( args.Count >= 3 )
                Enum.TryParse( args[2], true, out team );

            SpawnMasterWithBody( args[0], args[1], master.GetBody().transform.position, team, master.inventory );
        }

        [ConCommand( commandName = "mod_deployables_give" )]
        public static void CCGiveDeployables( ConCommandArgs args )
        {
            args.CheckArgumentCount( 1 );
            ItemIndex item;
            int count = 1;
            if ( !Enum.TryParse( args[0], true, out item ) )
            {
                Debug.Log( "Invalid item" );
                return;
            }
            if ( args.Count > 1 )
                int.TryParse( args[1], out count ); // ignore if it's not successfull, just give it 1.
            CharacterMaster master = args.senderMasterObject.GetComponent<CharacterMaster>();
            List<DeployableInfo> deployables = (List<DeployableInfo>)master.GetType().GetField( "deployablesList", BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance ).GetValue( master );

            if ( deployables == null ) // this list can actually be null sometimes
                return;

            foreach ( DeployableInfo d in deployables )
                d.deployable.GetComponent<Inventory>().GiveItem( item, count );
        }

        [ConCommand( commandName = "mod_list_masters" )]
        public static void CCListMasters( ConCommandArgs args )
        {
            using ( StreamWriter sw = new StreamWriter( "masters.txt" ) )
                foreach ( CharacterMaster master in MasterCatalog.allMasters )
                    sw.WriteLine( master.name );
        }

        [ConCommand( commandName = "mod_list_bodies" )]
        public static void CCListBodies( ConCommandArgs args )
        {
            using ( StreamWriter sw = new StreamWriter( "bodies.txt" ) )
                foreach ( GameObject body in BodyCatalog.allBodyPrefabs )
                    sw.WriteLine( body.name );
        }

        [ConCommand( commandName = "mod_list_gamemodes" )]
        public static void CCListGamemodes( ConCommandArgs args )
        {
            using ( StreamWriter sw = new StreamWriter( "gamemodes.txt" ) )
                foreach ( Run gm in (Run[])typeof( GameModeCatalog ).GetField( "indexToPrefabComponents", BindingFlags.NonPublic | BindingFlags.Static ).GetValue( null ) )
                    sw.WriteLine( gm.name + " " + gm.nameToken );
        }
    }
}