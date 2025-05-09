using UnityEngine;
namespace Invector.Throw
{
    [vClassHeader("THROW COLLECTABLE", false)]
    public class vThrowCollectable : vMonoBehaviour
    {
        public string throwableName;
        public int amount = 1;
        public bool destroyAfter = true;

        public UnityEngine.Events.UnityEvent onCanCollect;
        public UnityEngine.Events.UnityEvent onCanCollectFromInventory;
        public UnityEngine.Events.UnityEvent onIsStandAloneManager;
        public UnityEngine.Events.UnityEvent onIsInventoryManager;
        public UnityEngine.Events.UnityEvent onCollectObject;
        public UnityEngine.Events.UnityEvent onReachMaxObjects;
        public UnityEngine.Events.UnityEvent onEnterTrigger;
        public UnityEngine.Events.UnityEvent onExitTrigger;

        vThrowManagerBase throwManager;
        bool isInventory => throwManager != null && throwManager is vThrowManagerInventory;

        Collider _throwManagerCollider;

        protected bool canCollect;

        bool _isInventory;
        private void OnTriggerStay(Collider other)
        {
            Debug.Log(other.name);
            if (throwManager != null)
            {
                UpdateThrowInfo(false);
                return;
            }

            if (other.gameObject.CompareTag("Player"))
            {
                //throwManager = other.GetComponent<vThrowManagerBase>();
                throwManager = other.transform.root.GetComponent<vThrowManagerBase>();
            }

            if (throwManager != null)
            {
                _throwManagerCollider = other;
                onEnterTrigger.Invoke();
            }

            UpdateThrowInfo(true);
        }

        private void OnTriggerExit(Collider other)
        {
            if (_throwManagerCollider != null && other.gameObject == _throwManagerCollider.gameObject)
            {
                _throwManagerCollider = null;
                throwManager = null;
                onExitTrigger.Invoke();
            }
        }

        public void UpdateThrowInfo(bool firstEnter)
        {
            if (throwManager != null)
            {
                if (isInventory != _isInventory || firstEnter)
                {
                    if (isInventory)
                    {
                        onIsInventoryManager.Invoke();
                    }
                    else onIsStandAloneManager.Invoke();
                    _isInventory = isInventory;
                }

                var _canCollect = throwManager.CanCollectThrowable(throwableName, out int remainingAmount);
                if (canCollect != _canCollect || firstEnter)
                {
                    canCollect = _canCollect;
                    if (_canCollect)
                    {
                        if (isInventory) onCanCollectFromInventory.Invoke();
                        else onCanCollect.Invoke();
                    }
                    else
                    {
                    }
                }
            }
        }

        public void UpdateThrowObj()
        {
            if (throwManager.CanCollectThrowable(throwableName, out int remainingAmount))
            {
                throwManager.OnCollectThrowable(throwableName, amount);

                if (amount <= remainingAmount)
                {
                    if (destroyAfter) Destroy(this.gameObject);
                }
                else
                {
                    amount -= remainingAmount;
                    if (amount <= 0)
                    {
                        if (destroyAfter) Destroy(this.gameObject);
                    }
                }
                onCollectObject.Invoke();
            }
            else
            {
                onReachMaxObjects.Invoke();
            }
        }
    }
}