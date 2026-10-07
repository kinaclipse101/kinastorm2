using System;
using System.Linq;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.Achievements;
using UnityEngine.SceneManagement;
using UnityEngine;
using MSU;
using Rewired.ComponentControls.Effects;
using SS2;
using SS2.Components;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using static UnityEngine.Object;
using Object = UnityEngine.Object;

namespace SS2.Unlocks.Chirr
{
    public sealed class ChirrDevilAchievement : BaseAchievement
    {
        public static bool active;
        
        public override void OnInstall()
        {
            base.OnInstall();
            base.SetServerTracked(true);
            active = true;
            BefriendAltarController.grant += ListenForAltarDeath;
        }

        private void ListenForAltarDeath()
        {
            Grant();
        }

        public override void OnUninstall()
        {
            base.OnUninstall();
            base.SetServerTracked(false);
            active = false;
            BefriendAltarController.grant -= ListenForAltarDeath;
        }
        
        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("ChirrBody");
        }
        
        private class ChirrDevilServerAchievement : BaseServerAchievement
        {
            public override void OnInstall()
            {
                base.OnInstall();
                Stage.onServerStageBegin += StageOnServerStageBegin;
            }

            private void StageOnServerStageBegin(Stage stage)
            {
                if (stage.sceneDef.baseSceneName != "foggyswamp") return;

                GameObject skeletonObject = GameObject.Find("HOLDER: Hidden Altar Stuff/AltarCenter/AltarSkeletonBody");
                skeletonObject.GetComponent<GameObjectUnlockableFilter>().active = false;
                skeletonObject.SetActive(true);
            }

            public override void OnUninstall()
            {
                base.OnUninstall();
                Stage.onServerStageBegin -= StageOnServerStageBegin;
            }
        }
        
        public static HurtBox SearchForSkeletonAltar(Ray aimRay, BullseyeSearch bullseyeSearch)
        {
            TeamMask filter = TeamMask.none;
            filter.AddTeam(TeamIndex.Neutral);
            bullseyeSearch.teamMaskFilter = filter;
            bullseyeSearch.filterByLoS = true;
            bullseyeSearch.searchOrigin = aimRay.origin;
            bullseyeSearch.searchDirection = aimRay.direction;
            bullseyeSearch.sortMode = BullseyeSearch.SortMode.Angle;
            bullseyeSearch.maxDistanceFilter = 70;
            bullseyeSearch.maxAngleFilter = 75;
            bullseyeSearch.RefreshCandidates();
            HurtBox[] hurtBoxes = bullseyeSearch.GetResults().ToArray();
            foreach(HurtBox hurtBox in hurtBoxes)
            {
                if(hurtBox.healthComponent.body.baseNameToken != "ALTARSKELETON_BODY_NAME") continue;

                return hurtBox;
            }

            return null;
        }
    }   
}

public class BefriendAltarController : MonoBehaviour
{
    public CharacterBody body;
    public CharacterMaster master;
    public ChirrFriendTracker chirrTracker;
    public bool active;
    public float timer;
    private RotateAroundAxis lightRotater;
    private Light light;
    public static Action grant;
            
    public void OnEnable()
    {
        body = GetComponent<CharacterBody>();
        lightRotater = transform.Find("ModelBase/LightSpinner").gameObject.GetComponent<RotateAroundAxis>();
        light = transform.Find("ModelBase/LightSpinner/LightSpinner/Point Light").gameObject.GetComponent<Light>();
        light.gameObject.GetComponent<FlickerLight>().enabled = false;
        GameObject masterObj = Instantiate(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleMaster.prefab").WaitForCompletion());
        
        master = masterObj.GetComponent<CharacterMaster>();
        master.bodyPrefab = body.gameObject;
        master.teamIndex = TeamIndex.Player;
        
        if(NetworkServer.active)
            NetworkServer.Spawn(masterObj);
    }

    public void Befriend()
    {
        body.teamComponent.teamIndex = TeamIndex.Player;
        body.bodyFlags -= CharacterBody.BodyFlags.Masterless;
        
        master.bodyInstanceObject = body.gameObject;
        body.masterObject = master.gameObject;
        body._master = master;
        
        Chat.SendBroadcastChat(new Chat.SimpleChatMessage { baseToken = "SS2_ACHIEVEMENT_CHIRR_DEMON_FOREVERMORE" }); 
        
        master.minionOwnership.SetOwner(chirrTracker.characterBody.master);
        master.inventory.GiveItemPermanent(SS2Content.Items.ChirrFriendHelper);
        active = true;
    }

    public void Update()
    {
        if(!active) return;
        
        timer += Time.deltaTime;
        light.intensity += Time.deltaTime * 30;
        light.range += Time.deltaTime * 30;
        lightRotater.slowRotationSpeed += Time.deltaTime * 60;
        
        if (timer > 1.5f)
        {
            master.TrueKill();
        }
    }

    public void OnDisable()
    {
        Util.PlaySound("Play_elite_haunt_spawn", GameObject.Find("HOLDER: Hidden Altar Stuff/AltarCenter"));
        grant.Invoke();
    }
}