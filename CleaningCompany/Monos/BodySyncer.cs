using Unity.Netcode;
using UnityEngine;

namespace CleaningCompany.Monos
{
    internal class BodySyncer : NetworkBehaviour
    {
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if(IsHost || IsServer) 
            {
                PhysicsProp prop = GetComponent<PhysicsProp>();
                if (prop.scrapValue != 0) return; // prevent saved scrap from being rerolled;
                int price = Random.Range(prop.itemProperties.minValue, prop.itemProperties.maxValue);
                SyncDetailsClientRpc(price, new NetworkBehaviourReference(prop));
            }
        }

        [ClientRpc]
        void SyncDetailsClientRpc(int price, NetworkBehaviourReference netRef)
        {
            netRef.TryGet(out PhysicsProp prop);
            if (prop != null)
            {
                prop.scrapValue = price;
                prop.itemProperties.creditsWorth = price;
                prop.GetComponentInChildren<ScanNodeProperties>().subText = $"Value: ${price}";
                Debug.Log("Successfully synced body values");
            }
            else Debug.LogError("Failed to resolve network reference!");
        }
    }
}
