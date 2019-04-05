using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2;
using RoR2.CharacterAI;
using System.Reflection;

namespace RoRMod
{
    // build #3703355, the devs removed their own debug commands. lets reimplement them ourself! James 04/03/19
    public class OldCommands
    {

        // Token: 0x06000D11 RID: 3345 RVA: 0x00052C74 File Offset: 0x00050E74
        [ConCommand( commandName = "god", flags = ( ConVarFlags.ExecuteOnServer ), helpText = "Toggles god mode on the sender." )]
        private static void CCGod( ConCommandArgs args )
        {
            if ( args.senderMasterObject )
            {
                CharacterMaster component = args.senderMasterObject.GetComponent<CharacterMaster>();
                if ( component )
                {
                    bool god = (bool)component.GetType().GetField( "godMode", BindingFlags.NonPublic | BindingFlags.Instance ).GetValue( component );
                    component.GetType().GetField( "godMode", BindingFlags.NonPublic | BindingFlags.Instance ).SetValue( component, !god );
                    component.GetBody().healthComponent.godMode = god;
                }
            }
        }

        // Token: 0x06000D18 RID: 3352 RVA: 0x00052FB0 File Offset: 0x000511B0
        [ConCommand( commandName = "give_money", flags = ( ConVarFlags.ExecuteOnServer ), helpText = "Gives the specified amount of money to the sender." )]
        private static void CCGiveMoney( ConCommandArgs args )
        {
            if ( args.Count == 0 )
            {
                return;
            }
            if ( args.senderMasterObject )
            {
                CharacterMaster component = args.senderMasterObject.GetComponent<CharacterMaster>();
                if ( component )
                {
                    try
                    {
                        int num = 1;
                        if ( args.Count > 0 )
                        {
                            TextSerialization.TryParseInvariant( args[0], out num );
                        }
                        component.money += (uint)num;
                    }
                    catch ( ArgumentException )
                    {
                    }
                }
            }
        }

        // Token: 0x06000D12 RID: 3346 RVA: 0x00052CDC File Offset: 0x00050EDC
        [ConCommand( commandName = "zoom", flags = ( ConVarFlags.ExecuteOnServer ), helpText = "Gives a bunch of items to help travel the map." )]
        private static void CCZoom( ConCommandArgs args )
        {
            if ( args.senderMasterObject )
            {
                Inventory component = args.senderMasterObject.GetComponent<Inventory>();
                if ( component )
                {
                    component.GiveItem( ItemIndex.Hoof, 30 );
                    component.GiveItem( ItemIndex.Feather, 100 );
                }
            }
        }

        // Token: 0x06000D13 RID: 3347 RVA: 0x00052D20 File Offset: 0x00050F20
        [ConCommand( commandName = "unzoom", flags = ( ConVarFlags.ExecuteOnServer ), helpText = "Removes the effects of zoom" )]
        private static void CCUnzoom( ConCommandArgs args )
        {
            if ( args.senderMasterObject )
            {
                Inventory component = args.senderMasterObject.GetComponent<Inventory>();
                if ( component )
                {
                    component.GiveItem( ItemIndex.Hoof, -30 );
                    component.GiveItem( ItemIndex.Feather, -100 );
                }
            }
        }

        // Token: 0x06000D14 RID: 3348 RVA: 0x00052D64 File Offset: 0x00050F64
        [ConCommand( commandName = "give_random_items", flags = ( ConVarFlags.ExecuteOnServer ), helpText = "Gives a random set of items. It will give approximately 80% white, 19% green, 1% red." )]
        private static void CCGiveRandomItems( ConCommandArgs args )
        {
            if ( args.Count == 0 )
            {
                return;
            }
            if ( args.senderMasterObject )
            {
                Inventory component = args.senderMasterObject.GetComponent<Inventory>();
                if ( component )
                {
                    try
                    {
                        int num;
                        TextSerialization.TryParseInvariant( args[0], out num );
                        if ( num > 0 )
                        {
                            WeightedSelection<List<PickupIndex>> weightedSelection = new WeightedSelection<List<PickupIndex>>( 8 );
                            weightedSelection.AddChoice( Run.instance.availableTier1DropList, 80f );
                            weightedSelection.AddChoice( Run.instance.availableTier2DropList, 19f );
                            weightedSelection.AddChoice( Run.instance.availableTier3DropList, 1f );
                            for ( int i = 0; i < num; i++ )
                            {
                                List<PickupIndex> list = weightedSelection.Evaluate( UnityEngine.Random.value );
                                component.GiveItem( list[UnityEngine.Random.Range( 0, list.Count )].itemIndex, 1 );
                            }
                        }
                    }
                    catch ( ArgumentException )
                    {
                    }
                }
            }
        }

        // Token: 0x06000D15 RID: 3349 RVA: 0x00052E54 File Offset: 0x00051054
        [ConCommand( commandName = "give_item", flags = ( ConVarFlags.ExecuteOnServer ), helpText = "Gives the named item to the sender. Second argument can specify stack." )]
        private static void CCGiveItem( ConCommandArgs args )
        {
            if ( args.Count == 0 )
            {
                return;
            }
            if ( args.senderMasterObject )
            {
                Inventory component = args.senderMasterObject.GetComponent<Inventory>();
                if ( component )
                {
                    try
                    {
                        int count = 1;
                        if ( args.Count > 1 )
                        {
                            TextSerialization.TryParseInvariant( args[1], out count );
                        }
                        component.GiveItem( (ItemIndex)Enum.Parse( typeof( ItemIndex ), args[0] ), count );
                    }
                    catch ( ArgumentException )
                    {
                    }
                }
            }
        }

        // Token: 0x06000D16 RID: 3350 RVA: 0x00052EE4 File Offset: 0x000510E4
        [ConCommand( commandName = "inventory_clear", flags = ( ConVarFlags.ExecuteOnServer ), helpText = "Clears the sender's inventory." )]
        private static void CCClearItems( ConCommandArgs args )
        {
            if ( args.Count == 0 )
            {
                return;
            }
            if ( args.senderMasterObject )
            {
                Inventory component = args.senderMasterObject.GetComponent<Inventory>();
                if ( component )
                {
                    for ( ItemIndex itemIndex = ItemIndex.Syringe; itemIndex < ItemIndex.Count; itemIndex++ )
                    {
                        component.ResetItem( itemIndex );
                    }
                    component.SetEquipmentIndex( EquipmentIndex.None );
                }
            }
        }

        // Token: 0x06000D17 RID: 3351 RVA: 0x00052F3C File Offset: 0x0005113C
        [ConCommand( commandName = "give_equipment", flags = ( ConVarFlags.ExecuteOnServer ), helpText = "Gives the named equipment to the sender." )]
        private static void CCGiveEquipment( ConCommandArgs args )
        {
            if ( args.Count == 0 )
            {
                return;
            }
            if ( args.senderMasterObject )
            {
                Inventory component = args.senderMasterObject.GetComponent<Inventory>();
                if ( component )
                {
                    try
                    {
                        component.SetEquipmentIndex( (EquipmentIndex)Enum.Parse( typeof( EquipmentIndex ), args[0] ) );
                    }
                    catch ( ArgumentException )
                    {
                    }
                }
            }
        }

        // Token: 0x06001490 RID: 5264 RVA: 0x00070DF4 File Offset: 0x0006EFF4
        [ConCommand( commandName = "run_set_time", flags = ( ConVarFlags.SenderMustBeServer ), helpText = "Sets the time of the current run." )]
        private static void CCRunSetTime( ConCommandArgs args )
        {
            if ( !Run.instance )
            {
                throw new ConCommandException( "No run is currently in progress." );
            }
            args.CheckArgumentCount( 1 );
            float networkfixedTime;
            if ( TextSerialization.TryParseInvariant( args[0], out networkfixedTime ) )
            {
                Run.instance.NetworkfixedTime = networkfixedTime;
            }
        }
    }
}
