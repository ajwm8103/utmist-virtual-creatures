using System;
using UnityEngine;

namespace StylizedWater2.UnderwaterRendering
{
	//Component requires to be added to a GameObject with a Collider (trigger checkbox enabled)
	public class UnderwaterTrigger : MonoBehaviour
	{
		[Tooltip("The tag of the object that should be considered the camera or the player")]
		public string triggerTag = "MainCamera";
		
		[Tooltip("If enabled, rendering will also be toggled based on triggers.\n\nThis way the camera can go below the (last set) water level without activating rendering (eg. stairs running down, next to a swimming pool)")]
		public bool toggleRendering = true;

		[Space]

		[Tooltip("Change the currently active water material when this volume is triggered. You can leave the material field empty, in case every water body uses the same material anyway, just at different water levels")]
		public Material waterMaterial;
		[Tooltip("When the Main Camera enters the trigger volume, assign the water level below to the Underwater Renderer")]
		public bool changeWaterLevel = true;
		public bool useTransformForWaterlevel;
		public float waterLevel;
		
		private void Start()
		{
			//Making sure that the rendering is disabled until the camera first enters a trigger.
			if (toggleRendering) UnderwaterRenderer.EnableRendering = false;
			
			//This is just to ensure that by default the camera is never considered underwater
			if (changeWaterLevel) UnderwaterRenderer.SetCurrentWaterLevel(-999f);
		}

		private float GetWaterLevel() => useTransformForWaterlevel ? this.transform.position.y : waterLevel;

		private void OnTriggerEnter(Collider other)
		{
			//Note that in order for the camera to react to triggers it also needs to have a trigger on its GameObject, as well as a RigidBody component (with Kinematic enabled, so it doesn't fall)
			if (!other.CompareTag(triggerTag)) return;

			if (toggleRendering) UnderwaterRenderer.EnableRendering = true;
			
			UnderwaterRenderer.SetCurrentWaterLevel(GetWaterLevel());
			
			if (waterMaterial) UnderwaterRenderer.SetCurrentWaterMaterial(waterMaterial);
		}

		private void OnTriggerStay(Collider other)
		{
			OnTriggerEnter(other);
		}

		private void OnTriggerExit(Collider other)
		{
			if (!other.CompareTag(triggerTag)) return;

			if (toggleRendering) UnderwaterRenderer.EnableRendering = false;
		}

		private void OnDrawGizmosSelected()
		{
			BoxCollider boxCollider = GetComponent<BoxCollider>();

			if (boxCollider)
			{
				UnderwaterRenderer.DrawWaterLevelGizmo(this.transform.TransformPoint(boxCollider.center), boxCollider.size, GetWaterLevel());
			}
		}
	}
}
