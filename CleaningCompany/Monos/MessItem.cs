using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

namespace CleaningCompany.Monos
{
    internal class MessItem : NetworkBehaviour
    {
        Animator anim;
        public GameObject loot;
        public string toolType;
        InteractTrigger trig;
        AudioSource audio;
        public AudioClip cleanNoise;

        void Awake()
        {
            trig = GetComponent<InteractTrigger>();
            trig.onInteract.AddListener(CleanMess);
            trig.onStopInteract.AddListener(StopMess);
            trig.onInteractEarly.AddListener(PlayMess);

            anim = GetComponentInChildren<Animator>();
            audio = GetComponent<AudioSource>();
            audio.clip = cleanNoise;
        }

        void Update()
        {
            PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
            if (player == null)
            {
                trig.interactable = false;
                return;
            }
            if (player.currentlyHeldObjectServer == null)
            {
                trig.interactable = false;
                return;
            }
            ToolItem tool = player.currentlyHeldObjectServer.GetComponent<ToolItem>();
            if (tool == null)
            {
                trig.interactable = false;
                return;
            }
            if (tool.toolType == toolType) trig.interactable = true;
            else trig.interactable = false;
        }


        void CleanMess(PlayerControllerB player)
        {
            Debug.Log("Clean");
            SpawnLootServerRpc();
            audio.Stop();
            audio.volume = 0f;
        }

        void StopMess(PlayerControllerB player)
        {
            Debug.Log("Stopping");
                anim.SetTrigger("Cleaning");
            anim.SetTrigger("Idle");
            audio.Pause();
        }

        void PlayMess(PlayerControllerB player)
        {
            Debug.Log(anim.GetCurrentAnimatorStateInfo(0));
            if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Cleaning"))
            {
                anim.SetTrigger("Cleaning");
            }
            if (!audio.isPlaying)
            {
                audio.Play();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        void SpawnLootServerRpc()
        {
            GameObject go = Instantiate(loot, transform.position + Vector3.up, Quaternion.identity);
            go.GetComponent<NetworkObject>().Spawn();
            PhysicsProp prop = go.GetComponent<PhysicsProp>();
            int price = Random.Range(prop.itemProperties.minValue, prop.itemProperties.maxValue);
            SyncDetailsClientRpc(price, new NetworkBehaviourReference(prop));
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
                Debug.Log("Successfully synced trash values");
            }
            else Debug.LogError("Failed to resolve network reference!");
            gameObject.SetActive(false);
        }
    }
}
