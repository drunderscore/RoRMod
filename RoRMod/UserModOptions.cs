using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2;
using RoR2.ConVar;
using RoR2.Networking;

namespace RoRMod
{
    public class UserModOptions : MonoBehaviour
    {
        public void Start()
        {

        }

        public void Update()
        {
            if ( Input.GetKeyDown( KeyCode.Z ) )
            {
                CharacterMaster master = GetComponent<CharacterMaster>();
                if ( master && CameraRigController.readOnlyInstancesList.Count >= 1 )
                    TeleportHelper.TeleportBody( master.GetBody(), CameraRigController.readOnlyInstancesList[0].crosshairWorldPosition );
            }
        }

        public void FixedUpdate()
        {

        }
    }
}
