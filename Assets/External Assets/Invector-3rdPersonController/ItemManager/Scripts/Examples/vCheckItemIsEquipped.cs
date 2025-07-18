using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Invector.vItemManager
{
    [vClassHeader("Check If Item Is Equipped", openClose = false)]
    public class vCheckItemIsEquipped : vMonoBehaviour
    {
        public vItemManager itemManager;
        public bool getInParent = true;

        [FormerlySerializedAs("itemChecks")]
        public List<CheckItemIDEvent> itemIDEvents;
        public List<CheckItemTypeEvent> itemTypeEvents;

        void Awake()
        {
            if (!itemManager)
            {
                if (getInParent)
                    itemManager = GetComponentInParent<vItemManager>();
                else
                    itemManager = GetComponent<vItemManager>();

                if (itemManager != null)
                {
                    itemManager.onEquipItem.AddListener(CheckIsEquipped);
                    itemManager.onUnequipItem.AddListener(CheckIsEquipped);
                    Debug.Log($"[vCheckItemIsEquipped] ItemManager found and listeners attached.");
                }
                else
                {
                    Debug.LogWarning("[vCheckItemIsEquipped] ItemManager not found!");
                }
            }
        }

        private void CheckIsEquipped(vEquipArea area, vItem item)
        {
            //Debug.Log($"[vCheckItemIsEquipped] Checking equipment change: {item.name} in {area.name}");

            for (int i = 0; i < itemIDEvents.Count; i++)
            {
                CheckItemIDEvent check = itemIDEvents[i];
                CheckItemID(check);
            }

            for (int i = 0; i < itemTypeEvents.Count; i++)
            {
                CheckItemTypeEvent check = itemTypeEvents[i];
                CheckItemType(check);
            }
        }

        private void CheckItemID(CheckItemIDEvent check)
        {
            // Debug each ID being checked
            foreach (int id in check._itemsID)
            {
                bool individualCheck = itemManager.ItemIsEquipped(id);
                //Debug.Log($"[CheckItemID] Checking item ID: {id} => IsEquipped: {individualCheck}");
            }

            bool _isEquipped = check._itemsID.Exists(t => itemManager.ItemIsEquipped(t));
            //Debug.Log($"[CheckItemID] Group '{check.name}' - Previous isEquipped: {check.isEquipped}, Current: {_isEquipped}");

            //Debug.Log($"[CheckItemID] Is Equipped: {_isEquipped} | Was Equipped: {check.isEquipped}");

            if (_isEquipped != check.isEquipped)
            {
                check.isEquipped = _isEquipped;

                if (check.isEquipped)
                {
                    //Debug.Log($"[CheckItemID] => Item ID group '{check.name}' is now EQUIPPED.");
                    check.onIsItemEquipped.Invoke();
                }
                else
                {
                    //Debug.Log($"[CheckItemID] => Item ID group '{check.name}' is now UNEQUIPPED.");
                    check.onIsItemUnequipped.Invoke();
                }
            }
            else
            {
                //Debug.Log($"[CheckItemID] => No change for item ID group '{check.name}'. Still {(check.isEquipped ? "EQUIPPED" : "UNEQUIPPED")}.");
            }
        }


        private void CheckItemType(CheckItemTypeEvent check)
        {
            // Log each item type and whether it's equipped
            foreach (var type in check.itemTypes)
            {
                bool individualCheck = itemManager.ItemTypeIsEquipped(type);
                //Debug.Log($"[CheckItemType] Checking item type: {type} => IsEquipped: {individualCheck}");
            }
            //Debug.Log("check.isEquipped " + check.isEquipped);

            bool _isEquipped = check.itemTypes.Exists(t => itemManager.ItemTypeIsEquipped(t));
            //Debug.Log($"[CheckItemType] Group '{check.name}' - Previous isEquipped: {check.isEquipped}, Current: {_isEquipped}");

            if (_isEquipped != check.isEquipped)
            {
                check.isEquipped = _isEquipped;

                if (check.isEquipped)
                {
                    //Debug.Log($"[CheckItemType] => Item type group '{check.name}' is now EQUIPPED.");
                    check.onIsItemEquipped.Invoke();
                }
                else
                {
                    //Debug.Log($"[CheckItemType] => Item type group '{check.name}' is now UNEQUIPPED.");
                    check.onIsItemUnequipped.Invoke();
                }
            }
            else
            {
                //Debug.Log($"[CheckItemType] => No change for item type group '{check.name}'. Still {(check.isEquipped ? "EQUIPPED" : "UNEQUIPPED")}.");
            }


        }
        public void UnEquipingUI()
        {
            for (int i = 0; i < itemTypeEvents.Count; i++)
            {
                CheckItemTypeEvent _vCheckItemIsEquipped = new CheckItemTypeEvent();
                _vCheckItemIsEquipped = itemTypeEvents[i];
                _vCheckItemIsEquipped.isEquipped = false;
                itemTypeEvents[i] = _vCheckItemIsEquipped;
                //CheckItemType(check);
            }
        }


        [System.Serializable]
        public class CheckItemIDEvent
        {
            public string name;
            public List<int> _itemsID;
            public UnityEngine.Events.UnityEvent onIsItemEquipped, onIsItemUnequipped;
            internal bool isEquipped;
        }

        [System.Serializable]
        public class CheckItemTypeEvent
        {
            public string name;
            public List<vItemType> itemTypes;
            public UnityEngine.Events.UnityEvent onIsItemEquipped, onIsItemUnequipped;
            internal bool isEquipped;
        }
    }
}
