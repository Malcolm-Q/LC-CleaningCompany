
using GameNetcodeStuff;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace CleaningCompany.Monos
{
    [RequireComponent(typeof(AudioSource))]
    internal class CabinetScript : NetworkBehaviour
    {
        Animator leftDoor, rightDoor;
        AudioSource audio;
        AudioClip open, close;
        InteractTrigger leftTrig, rightTrig, broomTrig, vacTrig, mopTrig, garbTrig;
        bool leftClosed, rightClosed;
        Dictionary<bool, string> doorTip = new Dictionary<bool, string>()
        {
            {true, "Open : [E]" },
            {false, "Close : [E]" }
        };

        void Start()
        {
            // SFX
            audio = GetComponent<AudioSource>();
            open = Plugin.instance.open;
            close = Plugin.instance.close;

            // DOORS
            leftDoor = transform.GetChild(0).GetChild(1).GetComponent<Animator>();
            rightDoor = transform.GetChild(0).GetChild(0).GetComponent<Animator>();
            leftTrig = leftDoor.GetComponent<InteractTrigger>();
            leftTrig.onInteract.AddListener(ChangeLeftDoorState);
            rightTrig = rightDoor.GetComponent<InteractTrigger>();
            rightTrig.onInteract.AddListener(ChangeRightDoorState);

            // ITEM TRIGGERS
            broomTrig = transform.GetChild(0).GetChild(2).GetComponent<InteractTrigger>();
            broomTrig.onInteract.AddListener(GrabBroom);
            mopTrig = transform.GetChild(0).GetChild(3).GetComponent<InteractTrigger>();
            mopTrig.onInteract.AddListener(GrabMop);
            garbTrig = transform.GetChild(0).GetChild(4).GetComponent<InteractTrigger>();
            garbTrig.onInteract.AddListener(GrabGarb);
            vacTrig = transform.GetChild(0).GetChild(5).GetComponent<InteractTrigger>();
            vacTrig.onInteract.AddListener(GrabVac);
        }

        void GrabBroom(PlayerControllerB player)
        {
            SpawnToolServerRpc(1, player.transform.position);
        }
        void GrabVac(PlayerControllerB player)
        {
            SpawnToolServerRpc(3, player.transform.position);
        }
        void GrabMop(PlayerControllerB player)
        {
            SpawnToolServerRpc(0, player.transform.position);
        }
        void GrabGarb(PlayerControllerB player)
        {
            SpawnToolServerRpc(2, player.transform.position);
        }

        [ServerRpc(RequireOwnership = false)]
        void SpawnToolServerRpc(int toolID, Vector3 pos)
        {
            GameObject go = Instantiate(Plugin.instance.tools[toolID],pos,Quaternion.identity);
            NetworkObject netObj = go.GetComponent<NetworkObject>();
            netObj.Spawn();
            StartCoroutine(setParentToShip(netObj));
        }

        IEnumerator setParentToShip(NetworkObject netObj)
        {
            yield return new WaitForSeconds(0.5f);
            netObj.TrySetParent(transform.parent);
        }

        void ChangeLeftDoorState(PlayerControllerB player)
        {
            if(leftClosed) audio.PlayOneShot(open);
            else audio.PlayOneShot(close);
            if (IsHost || IsServer) TriggerDoorClientRpc(true);
            else TriggerDoorServerRpc(true);
        }

        void ChangeRightDoorState(PlayerControllerB player)
        {
            if (rightClosed) audio.PlayOneShot(open);
            else audio.PlayOneShot(close);
            if (IsHost || IsServer) TriggerDoorClientRpc(false);
            else TriggerDoorServerRpc(false);
        }

        [ServerRpc(RequireOwnership = false)]
        void TriggerDoorServerRpc(bool left)
        {
            TriggerDoorClientRpc(left);
        }

        [ClientRpc]
        void TriggerDoorClientRpc(bool left)
        {
            if (left)
            {
                leftDoor.SetTrigger("Play");
                leftClosed = !leftClosed;
                leftTrig.hoverTip = doorTip[leftClosed];
            }
            else
            {
                rightDoor.SetTrigger("Play");
                rightClosed = !rightClosed;
                rightTrig.hoverTip = doorTip[rightClosed];
            }
        }

    }
}
