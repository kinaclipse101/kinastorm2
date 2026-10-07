using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using RoR2;
using SS2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace RuneFoxMods
{
    class Utils
    {
        static public void PrintBodyCatalog()
        {
            Debug.Log("\nBodyCatalog");
            foreach (var bod in BodyCatalog.allBodyPrefabs)
            {
                Debug.Log(bod.name);
            }

            Debug.Log("\n");
        }

        static public void ReadChildren(GameObject parent, int tabs, bool includecomponents = true)
        {
            string tabstring = "";
            for (int x = 0; x < tabs; x++)
            {
                tabstring += "  ";
            }

            //Debug.Log("children: " + parent.transform.childCount);

            for (int i = 0; i < parent.transform.childCount; i++)
            {
                var child = parent.transform.GetChild(i);
                if (child == null)
                    return;

                string active = child.gameObject.activeInHierarchy.ToString();

                Debug.Log(tabstring + child.name + " " + active + " " + child.transform.position);
                if (includecomponents)
                {
                    ReadComponents(child.gameObject, tabs + 1);
                    Debug.Log(""); //new line
                }

                ReadChildren(child.gameObject, tabs + 1, includecomponents);
            }
        }

        static public void readheiarchy(GameObject parent, bool includeComponents = true)
        {
            //read game object heiarchy
            Debug.Log(parent.name);
            if (includeComponents)
            {
                ReadComponents(parent, 1);
                Debug.Log(""); //new line
            }

            ReadChildren(parent, 1, includeComponents);
        }

        static public void ReadComponents(GameObject obj, int tabs)
        {
            string tabstring = "";
            for (int x = 0; x < tabs; x++)
            {
                tabstring += "  ";
            }

            var list = obj.GetComponents(typeof(Component));
            foreach (var comp in list)
            {
                string Behavior_enable = "";
                //var beh = comp.GetComponent<Behaviour>();
                //if (beh)
                //  Behavior_enable = beh.isActiveAndEnabled.ToString();

                Debug.Log(tabstring + "Comp: " + comp.GetType().ToString() + "  " + Behavior_enable);
            }
        }

        static public void PrintAllPaths(GameObject parent)
        {
            //InstanceLogger.LogWarning("Start Print All Paths");
            Debug.Log(parent.name);
            PrintAllPathsInner(parent, parent.name);
            //InstanceLogger.LogWarning("End Print All Paths");
        }

        static private void PrintAllPathsInner(GameObject parent, string parent_string)
        {
            int childcount = parent.transform.childCount;
            for (int i = 0; i < childcount; i++)
            {
                var child = parent.transform.GetChild(i);

                //don't log _end transforms
                if (child.name.EndsWith("_end") == true)
                    break;

                string path_string = parent_string + "/" + child.name;
                Debug.Log("  " + child.name + "\t\t" + path_string);
                PrintAllPathsInner(child.gameObject, path_string);
            }
        }

        public static void PrintDynamicBone(DynamicBone DB)
        {
            Debug.Log("Root: " + DB.m_Root);
            Debug.Log("Damping: " + DB.m_Damping);
            Debug.Log("Damping Dist: " + DB.m_DampingDistrib);
            Debug.Log("Elasticity: " + DB.m_Elasticity);
            Debug.Log("Elasticity Dist: " + DB.m_ElasticityDistrib);
            Debug.Log("Stiffness: " + DB.m_Stiffness);
            Debug.Log("Stiffness Dist: " + DB.m_StiffnessDistrib);
            Debug.Log("Inert: " + DB.m_Inert);
            Debug.Log("Inert Dist: " + DB.m_InertDistrib);
            Debug.Log("Radius: " + DB.m_Radius);
            Debug.Log("Radius Dist: " + DB.m_RadiusDistrib);
            Debug.Log("End Length: " + DB.m_EndLength);
            Debug.Log("End Offset: " + DB.m_EndOffset);
            Debug.Log("Gravity: " + DB.m_Gravity);
            Debug.Log("Force: " + DB.m_Force);
            Debug.Log("FreezeAxis: " + DB.m_FreezeAxis);

            Debug.Log("Colliders: " + DB.m_Colliders.Count);
            foreach (var collider in DB.m_Colliders)
            {
                Debug.Log("\t Parent: " + collider.transform.name);
                Debug.Log("\t Direction: " + collider.m_Direction);
                Debug.Log("\t Center " + collider.m_Center);
                Debug.Log("\t Bound " + collider.m_Bound);
                Debug.Log("\t Radius " + collider.m_Radius);
                Debug.Log("\t Height " + collider.m_Height);
            }

            Debug.Log("Exclusions: " + DB.m_Exclusions.Count);
            foreach (var trans in DB.m_Exclusions)
            {
                Debug.Log("\t Transform: " + trans.name);
            }
        }

        //finds childlocator from the Character Body
        //should work with CharacterBody.GameObject and BodyCatalog.FindPrefab GameObjects
        static public ChildLocator GetChildLocator(GameObject body)
        {
            ChildLocator locator = null;
            var child1 = body.transform.GetChild(0);
            if (child1)
            {
                var child2 = child1.GetChild(0);
                if (child2)
                {
                    locator = child2.GetComponent<ChildLocator>();
                    if (locator)
                    {
                        Debug.Log("Locator Found");
                    }
                }
            }

            return locator;
        }


        //does a breadth first Search of Transform's children for a child with the given name 
        static public Transform FindChildInTree(Transform Root, string name)
        {
            Queue<Transform> transformQueue = new Queue<Transform>();

            transformQueue.Enqueue(Root);

            while (transformQueue.Count != 0)
            {
                var transform = transformQueue.Dequeue();

                //Debug.Log(transform.name);

                if (transform.name == name)
                    return transform;

                for (int i = 0; i < transform.childCount; i++)
                {
                    var child = transform.GetChild(i);
                    transformQueue.Enqueue(child);
                }
            }

            return null; //could not find the child in heiarchy
        }

        //removes the annoying "(Clone)" from instantiated objects
        static public string RemoveCloneNaming(string str)
        {
            return (str.Remove(str.Length - 7));
        }
    }
}

namespace RuneFoxMods.DynamicSkins
{
	public static class DynamicSkinHelpers
	{
		public static SkinnedMeshRenderer[] GetBaseSkinRenderers(GameObject modelObject)
		{
			SkinnedMeshRenderer[] componentsInChildren = modelObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
			List<SkinnedMeshRenderer> list = new List<SkinnedMeshRenderer>();
			SkinnedMeshRenderer[] array = componentsInChildren;
			foreach (SkinnedMeshRenderer val in array)
			{
				if ((Object)(object)((Component)val).transform.parent == (Object)(object)modelObject.transform)
				{
					list.Add(val);
				}
			}
			return list.ToArray();
		}

		public static Transform[] BoneArrayBuilder(Transform NewBoneRoot)
		{
			List<Transform> list = new List<Transform>();
			BoneArrayBuilderHelper(NewBoneRoot, list);
			return list.ToArray();
		}

		public static void BoneArrayBuilderHelper(Transform parent, List<Transform> list)
		{
			if (!((Object)parent).name.EndsWith("_end"))
			{
				list.Add(parent);
			}
			for (int i = 0; i < parent.childCount; i++)
			{
				BoneArrayBuilderHelper(parent.GetChild(i), list);
			}
		}

		public static Transform GetArmature(GameObject obj)
		{
			return GetArmatureHelper(obj);
		}

		public static Transform GetArmatureHelper(GameObject obj)
		{
			if (((Object)obj).name.ToLower().Contains("armature"))
			{
				return obj.transform;
			}
			for (int i = 0; i < obj.transform.childCount; i++)
			{
				Transform armatureHelper = GetArmatureHelper(((Component)obj.transform.GetChild(i)).gameObject);
				if (armatureHelper)
				{
					return armatureHelper;
				}
			}
			return null;
		}

		public static Transform GetTopParent(Transform obj)
		{
			Transform val = obj;
			while ((Object)(object)val.parent != (Object)null)
			{
				val = val.parent;
			}
			return val;
		}

		public static string GetPrevBoneInList(Transform targetBone, SkinnedMeshRenderer meshRenderer)
		{
			Transform[] bones = meshRenderer.bones;
			for (int i = 0; i <= bones.Length - 1; i++)
			{
				if (((Object)bones[i + 1]).name == ((Object)targetBone).name)
				{
					return ((Object)bones[i]).name;
				}
			}
			return null;
		}

		public static int GetBoneIndexInList(Transform targetBone, SkinnedMeshRenderer meshRenderer)
		{
			Transform[] bones = meshRenderer.bones;
			for (int i = 0; i <= bones.Length; i++)
			{
				if (((Object)bones[i]).name == ((Object)targetBone).name)
				{
					return i;
				}
			}
			return -1;
		}

		public static int GetPrevBoneIndexInList(Transform targetBone, SkinnedMeshRenderer meshRenderer)
		{
			Transform[] bones = meshRenderer.bones;
			for (int i = 0; i <= bones.Length - 1; i++)
			{
				if (((Object)bones[i + 1]).name == ((Object)targetBone).name)
				{
					return i;
				}
			}
			return -1;
		}
	}
	internal class DynamicSkinManager
	{
		internal Dictionary<string, SkinDef> SkinDefs = new Dictionary<string, SkinDef>();

		private GameObject LastModelObject;

		internal Dictionary<string, SortedList<int, Modification>> ModificationList = new Dictionary<string, SortedList<int, Modification>>();

		private Dictionary<GameObject, AppliedModifications> ModifiedObjects = new Dictionary<GameObject, AppliedModifications>();

		internal ManualLogSource InstanceLogger;

		internal void AddModification(string skinNameToken, int boneIndex, Modification modification)
		{
			if (!ModificationList.TryGetValue(skinNameToken, out var value))
			{
				value = new SortedList<int, Modification>();
				ModificationList.Add(skinNameToken, value);
			}
			value.Add(boneIndex, modification);
		}

		internal void AddSkinDef(SkinDef skinDef)
		{
			SkinDefs.Add(skinDef.nameToken, skinDef);
		}

		internal IEnumerator SkinDefApply(Func<SkinDef, GameObject, List<AssetReferenceT<Material>>, List<AssetReferenceT<Mesh>>, int, IEnumerator> orig, SkinDef self, GameObject modelObject, List<AssetReferenceT<Material>> loadedMaterials, List<AssetReferenceT<Mesh>> loadedMeshes, int unloadType)
		{
			IEnumerator result = orig(self, modelObject, loadedMaterials, loadedMeshes, unloadType);
			RemoveInvalidModelObjects();
			ModifiedObjects.TryGetValue(modelObject, out var value);
			try
			{
				if (!SkinDefs.TryGetValue(self.nameToken, out var _))
				{
					if (value != null)
					{
						ClearSkinModifications(LastModelObject, value);
					}
					return result;
				}
				if (value == null)
				{
					AppliedModifications appliedModifications = new AppliedModifications();
					ModifiedObjects.Add(modelObject, appliedModifications);
					ApplySkinModifications(self, modelObject, appliedModifications);
				}
			}
			catch (Exception ex)
			{
				InstanceLogger.LogWarning((object)"An error occured while adding accessories to a skin");
				InstanceLogger.LogError((object)ex);
			}
			return result;
		}

		private void RemoveInvalidModelObjects()
		{
			foreach (GameObject item in ModifiedObjects.Keys.Where((GameObject el) => !el).ToList())
			{
				ModifiedObjects.Remove(item);
			}
		}

		private void ClearSkinModifications(GameObject modelObject, AppliedModifications modifications)
		{
			while (modifications.BaseModelModifications.Count != 0)
			{
				Modification modification = modifications.BaseModelModifications.Pop();
				clearModification(modification, modelObject, modifications);
			}
			while (modifications.OtherModifications.Count != 0)
			{
				clearModification(modifications.OtherModifications[0], modelObject, modifications);
			}
			ModifiedObjects.Remove(modelObject);
		}

		private void ApplySkinModifications(SkinDef skindef, GameObject modelObject, AppliedModifications modifications)
		{
			CharacterModel component = modelObject.GetComponent<CharacterModel>();
			LastModelObject = modelObject;
			if (!ModificationList.TryGetValue(skindef.nameToken, out var value))
			{
				return;
			}
			foreach (KeyValuePair<int, Modification> item in value)
			{
				ApplyModification(modelObject, component, item.Value, modifications);
			}
		}

		private static void ApplyModification(GameObject modelObject, CharacterModel characterModel, Modification modification, AppliedModifications modifications)
		{
			_ = modification.bodyname;
			string parentname = modification.parentname;
			Transform val = Utils.FindChildInTree(modelObject.transform, parentname);
			GameObject val2;
			if (modification.affectsbasemodel)
			{
				val2 = Object.Instantiate<GameObject>(modification.prefab, val, false);
				((Object)val2).name = Utils.RemoveCloneNaming(((Object)val2).name);
				modification.instance = val2;
				modification.inst_armature = val2;
				ModificationApplyBones(modelObject, modification, modifications);
			}
			else
			{
				val2 = Object.Instantiate<GameObject>(modification.prefab, modelObject.transform, false);
				((Object)val2).name = Utils.RemoveCloneNaming(((Object)val2).name);
				modification.instance = val2;
				Transform armature = DynamicSkinHelpers.GetArmature(val2);
				((Component)armature).transform.SetParent(val, false);
				modification.inst_armature = ((Component)armature).gameObject;
			}
			modification.instance = val2;
			if (modification.dynamicBoneData != null)
			{
				ModificationApplyDynamicBones(modelObject, modification);
			}
			ModificationAddRenderers(val2, characterModel);
			modifications.OtherModifications.Add(modification);
		}

		private static void ModificationApplyBones(GameObject modelObject, Modification modification, AppliedModifications modifications)
		{
			SkinnedMeshRenderer[] baseSkinRenderers = DynamicSkinHelpers.GetBaseSkinRenderers(modelObject);
			List<Transform> list = baseSkinRenderers[0].bones.ToList();
			Transform[] array = DynamicSkinHelpers.BoneArrayBuilder(modification.instance.transform);
			list.InsertRange(modification.boneIndex, array);
			modification.boneCount = array.Length;
			SkinnedMeshRenderer[] array2 = baseSkinRenderers;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].bones = list.ToArray();
			}
			modifications.BaseModelModifications.Push(modification);
		}

		private static void ModificationApplyDynamicBones(GameObject modelObject, Modification modification)
		{
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_024e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0253: Unknown result type (might be due to invalid IL or missing references)
			DynamicBone val = (modification.inst_dynamicBone = modification.instance.AddComponent<DynamicBone>());
			List<DynamicBoneCollider> list = new List<DynamicBoneCollider>();
			foreach (DynamicBoneColliderData collider in modification.dynamicBoneData.m_Colliders)
			{
				DynamicBoneCollider val2 = ((Component)Utils.FindChildInTree(modelObject.transform, collider.m_parent_name)).gameObject.AddComponent<DynamicBoneCollider>();
				val2.m_Direction = collider.m_Direction;
				val2.m_Center = collider.m_Center;
				val2.m_Bound = collider.m_Bound;
				val2.m_Radius = collider.m_Radius;
				val2.m_Height = collider.m_Height;
				list.Add(val2);
			}
			modification.inst_DB_colliders = list;
			Transform root = (val.m_Root = Utils.FindChildInTree(modification.inst_armature.transform, modification.dynamicBoneData.m_Root));
			val.m_Damping = modification.dynamicBoneData.m_Damping;
			val.m_DampingDistrib = modification.dynamicBoneData.m_DampingDistrib;
			val.m_Elasticity = modification.dynamicBoneData.m_Elasticity;
			val.m_ElasticityDistrib = modification.dynamicBoneData.m_ElasticityDistrib;
			val.m_Stiffness = modification.dynamicBoneData.m_Stiffness;
			val.m_StiffnessDistrib = modification.dynamicBoneData.m_StiffnessDistrib;
			val.m_Inert = modification.dynamicBoneData.m_Inert;
			val.m_InertDistrib = modification.dynamicBoneData.m_InertDistrib;
			val.m_Radius = modification.dynamicBoneData.m_Radius;
			val.m_RadiusDistrib = modification.dynamicBoneData.m_RadiusDistrib;
			val.m_EndLength = modification.dynamicBoneData.m_EndLength;
			val.m_EndOffset = modification.dynamicBoneData.m_EndOffset;
			val.m_Gravity = modification.dynamicBoneData.m_Gravity;
			val.m_Force = modification.dynamicBoneData.m_Force;
			val.m_Colliders = list;
			val.m_Exclusions = new List<Transform>();
			foreach (string exclusion in modification.dynamicBoneData.m_Exclusions)
			{
				Transform val3 = Utils.FindChildInTree(root, exclusion);
				if ((Object)(object)val3 != (Object)null)
				{
					val.m_Exclusions.Add(val3);
				}
				else
				{
					Debug.LogWarning((object)"Tried to exclude a transform that could not be found");
				}
			}
			val.m_FreezeAxis = modification.dynamicBoneData.m_FreezeAxis;
		}

		private static void ModificationAddRenderers(GameObject newPart, CharacterModel characterModel)
		{
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			SkinnedMeshRenderer[] componentsInChildren = newPart.GetComponentsInChildren<SkinnedMeshRenderer>(true);
			Array.Resize(ref characterModel.baseRendererInfos, characterModel.baseRendererInfos.Length + componentsInChildren.Length);
			if (componentsInChildren.Length != 0)
			{
				int num = componentsInChildren.Length;
				SkinnedMeshRenderer[] array = componentsInChildren;
				foreach (SkinnedMeshRenderer val in array)
				{
					characterModel.baseRendererInfos[characterModel.baseRendererInfos.Length - num] = new CharacterModel.RendererInfo
					{
						renderer = (Renderer)(object)componentsInChildren[^num],
						ignoreOverlays = false,
						defaultShadowCastingMode = (ShadowCastingMode)1,
						defaultMaterial = ((Renderer)val).sharedMaterial
					};
					num--;
				}
			}
		}

		private void clearModification(Modification modification, GameObject modelObject, AppliedModifications modifications)
		{
			if (modification.inst_DB_colliders != null)
			{
				foreach (DynamicBoneCollider inst_DB_collider in modification.inst_DB_colliders)
				{
					Object.Destroy((Object)(object)inst_DB_collider);
				}
			}
			if (modification.affectsbasemodel)
			{
				SkinnedMeshRenderer[] baseSkinRenderers = DynamicSkinHelpers.GetBaseSkinRenderers(modelObject);
				List<Transform> list = baseSkinRenderers[0].bones.ToList();
				list.RemoveRange(modification.boneIndex, modification.boneCount);
				SkinnedMeshRenderer[] array = baseSkinRenderers;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].bones = list.ToArray();
				}
			}
			Object.Destroy((Object)(object)modifications.OtherModifications[0].inst_dynamicBone);
			Object.Destroy((Object)(object)modifications.OtherModifications[0].inst_armature);
			Object.Destroy((Object)(object)modifications.OtherModifications[0].instance);
			if (!modifications.OtherModifications.Remove(modification))
			{
				InstanceLogger.LogError((object)"Skin Modification was not removed");
			}
		}
	}
	internal class Modification
	{
		public string prefabpath;

		public string bodyname;

		public string parentname;

		public GameObject prefab;

		public bool affectsbasemodel;

		public DynamicBoneData dynamicBoneData;

		public string parentSkinToken;

		public int boneIndex;

		public int boneCount;

		public GameObject instance;

		public GameObject inst_armature;

		public DynamicBone inst_dynamicBone;

		public List<DynamicBoneCollider> inst_DB_colliders = new List<DynamicBoneCollider>();

		public Modification(string PrefabPath, string ParentName, string BodyName, string ParentSkinToken, int BoneIndex, bool AffectsBaseModel, AssetBundle assetBundle)
		{
			bodyname = BodyName;
			prefabpath = PrefabPath;
			parentname = ParentName;
			parentSkinToken = ParentSkinToken;
			affectsbasemodel = AffectsBaseModel;
			boneIndex = BoneIndex;
			prefab = assetBundle.LoadAsset<GameObject>(prefabpath);
			if ((Object)(object)prefab == (Object)null)
			{
				Debug.LogWarning((object)("Asset at " + PrefabPath + " was not loaded"));
			}
		}
	}
	internal class AppliedModifications
	{
		public Stack<Modification> BaseModelModifications = new Stack<Modification>();

		public List<Modification> OtherModifications = new List<Modification>();
	}
	internal class DynamicBoneData
	{
		public string m_Root;

		public float m_Damping;

		public AnimationCurve m_DampingDistrib;

		public float m_Elasticity;

		public AnimationCurve m_ElasticityDistrib;

		public float m_Stiffness;

		public AnimationCurve m_StiffnessDistrib;

		public float m_Inert;

		public AnimationCurve m_InertDistrib;

		public float m_Radius;

		public AnimationCurve m_RadiusDistrib;

		public float m_EndLength;

		public Vector3 m_EndOffset;

		public Vector3 m_Gravity;

		public Vector3 m_Force;

		public List<DynamicBoneColliderData> m_Colliders;

		public List<string> m_Exclusions;

		public DynamicBone.FreezeAxis m_FreezeAxis;

		public DynamicBoneData(string root, float damping, AnimationCurve damping_dist, float elasticity, AnimationCurve elasticity_dist, float stiffness, AnimationCurve stiffness_dist, float inert, AnimationCurve inert_dist, float radius, AnimationCurve radius_dist, float end_length, Vector3 end_offset, Vector3 gravity, Vector3 force, List<DynamicBoneColliderData> colliders, List<string> exclusions, DynamicBone.FreezeAxis freeze_axis)
		{
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			m_Root = root;
			m_Damping = damping;
			m_DampingDistrib = damping_dist;
			m_Elasticity = elasticity;
			m_ElasticityDistrib = elasticity_dist;
			m_Stiffness = stiffness;
			m_StiffnessDistrib = stiffness_dist;
			m_Inert = inert;
			m_InertDistrib = inert_dist;
			m_Radius = radius;
			m_RadiusDistrib = radius_dist;
			m_EndLength = end_length;
			m_EndOffset = end_offset;
			m_Gravity = gravity;
			m_Force = force;
			m_Colliders = colliders;
			m_Exclusions = exclusions;
			m_FreezeAxis = freeze_axis;
		}
	}
	internal class DynamicBoneColliderData
	{
		public string m_parent_name;

		public DynamicBoneCollider.Direction m_Direction;

		public Vector3 m_Center;

		public DynamicBoneCollider.Bound m_Bound;

		public float m_Radius;

		public float m_Height;

		public DynamicBoneColliderData(string parent_name, DynamicBoneCollider.Direction direction, Vector3 Center, DynamicBoneCollider.Bound bound, float radius, float heaight)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			m_parent_name = parent_name;
			m_Direction = direction;
			m_Center = Center;
			m_Bound = bound;
			m_Radius = radius;
			m_Height = heaight;
		}
	}
}