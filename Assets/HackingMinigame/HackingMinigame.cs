using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HackingMinigame : MonoBehaviour
{
    #region Singleton

    public static HackingMinigame Instance;

    private void Awake()
    {
        Instance = this;
    }

    #endregion

    public TextMeshProUGUI accessLevelText;
    public Slider timeLeftSlider;
    public DOTweenAnimation bgRedTintAnimation;

    [Header("Timer Settings")]
    public float hackTimeLimit = 60f; // Time limit in seconds
    public bool isTimerRunning = false;
    public float warningThreshold = 0.2f; // Start warning animation when 20% of time remains
    public bool warningAnimationStarted = false;

    [Header("Player State")]
    public int accessLevel = 0; // This value determines if the player can pass special nodes

    [Header("Runtime")]
    public Node currentNode;
    public NodeContainer nodeContainer;

    public Action onReachedEndNode;
    public Action onHackFailed;

    [Header("UI Shake")]
    public UIShake uiShake;
    public float accessDeniedShakeStrength = 15f;
    public float accessDeniedShakeDuration = 0.3f;
    public float accessConsumedShakeStrength = 8f;
    public float accessConsumedShakeDuration = 0.2f;
    public float hackSuccessShakeStrength = 20f;
    public float hackSuccessShakeDuration = 0.8f;
    public float hackFailureShakeStrength = 25f;
    public float hackFailureShakeDuration = 1.0f;

    [Header("Tutorial System")]
    public TutorialManager tutorialManager;
    public bool useTutorial = true; //  yeh bool control karega tutorial ko

    private float currentTimeLeft;

    public int AccessLevel
    {
        get => accessLevel;
        set
        {
            accessLevel = value;
            accessLevelText.text = value.ToString();
        }
    }

    //private void Start()
    //{
    //    AccessLevel = accessLevel;      // to update text
    //    InitializeTimer();

    //    nodeContainer = GetComponentInChildren<NodeContainer>();
    //    nodeContainer.Init();
    //    SetCurrentNode(0, 0);

    //    onReachedEndNode += OnReachedEndNode;
    //    onHackFailed += OnHackFailed;

    //    // Auto-find UIShake component if not assigned
    //    if (uiShake == null)
    //    {
    //        uiShake = GetComponentInChildren<UIShake>();
    //    }

    //    // Setup tutorial system
    //    if (tutorialManager == null)
    //    {
    //        tutorialManager = GetComponentInChildren<TutorialManager>();
    //    }

    //    StartTimer();
    //}

    public void StartMiniGame()
    {
        AccessLevel = accessLevel;      // to update text
        InitializeTimer();

        nodeContainer = GetComponentInChildren<NodeContainer>();
        nodeContainer.Init();
        SetCurrentNode(0, 0);

        onReachedEndNode += OnReachedEndNode;
        onHackFailed += OnHackFailed;

        // Auto-find UIShake component if not assigned
        if (uiShake == null)
        {
            uiShake = GetComponentInChildren<UIShake>();
        }

        // Setup tutorial system
        if (tutorialManager == null)
        {
            tutorialManager = GetComponentInChildren<TutorialManager>();
        }
        if (useTutorial)
        {
            tutorialManager.tutorialUI.gameObject.SetActive(true);
        }
        else
        {
            tutorialManager.SkipTutorial();
        }

        StartTimer();
    }

    private void InitializeTimer()
    {
        currentTimeLeft = hackTimeLimit;
        timeLeftSlider.maxValue = hackTimeLimit;
        timeLeftSlider.value = hackTimeLimit;
        warningAnimationStarted = false;
    }

    private void StartWarningAnimation()
    {
        if (bgRedTintAnimation != null && !warningAnimationStarted)
        {
            warningAnimationStarted = true;
            bgRedTintAnimation.DOPlay();
            Debug.Log("Warning animation started - time is running low!");
        }
    }

    private void StartTimer()
    {
        isTimerRunning = true;
        StartCoroutine(CountdownTimer());
    }

    private IEnumerator CountdownTimer()
    {
        while (currentTimeLeft > 0f && isTimerRunning)
        {
            currentTimeLeft -= Time.deltaTime;
            timeLeftSlider.value = currentTimeLeft;

            // Check if we should start the warning animation
            if (!warningAnimationStarted && currentTimeLeft <= hackTimeLimit * warningThreshold)
            {
                StartWarningAnimation();
            }

            if (currentTimeLeft <= 0f)
            {
                currentTimeLeft = 0f;
                timeLeftSlider.value = 0f;
                onHackFailed?.Invoke();
                break;
            }

            yield return null;
        }
    }

    private void OnReachedEndNode()
    {
        isTimerRunning = false;
        StartCoroutine(AnimateHackSuccess());
    }

    private void OnHackFailed()
    {
        isTimerRunning = false;
        StartCoroutine(AnimateHackFailure());
    }

    private IEnumerator AnimateHackSuccess()
    {
        // Shake UI for success
        if (uiShake != null)
        {
            uiShake.Shake(hackSuccessShakeStrength, hackSuccessShakeDuration);
        }

        MessageLine.Instance.ShowMessage("Hack Successfull", Color.green);
        yield return new WaitForSeconds(1.0f);
        EndHack();
    }

    private IEnumerator AnimateHackFailure()
    {
        // Shake UI for failure
        if (uiShake != null)
        {
            uiShake.Shake(hackFailureShakeStrength, hackFailureShakeDuration);
        }

        MessageLine.Instance.ShowMessage("Hack Failed - Time's Up!", Color.red);
        yield return new WaitForSeconds(1.0f);
        EndHack();
    }

    private void SetCurrentNode(int row, int col)
    {
        if (currentNode != null)
        {
            currentNode.nodeCurrentVisual.SetActive(false);
        }
        currentNode = nodeContainer.GetNode(row, col);      // Set Starting Node
        currentNode.nodeCurrentVisual.SetActive(true);
    }

    public void TravelToNode(Node targetNode)
    {
        if (!IsConnected(currentNode, targetNode))
            return;

        if (!TryConsumeAccess(targetNode))
        {
            Debug.Log("Access denied.");
            MessageLine.Instance.ShowMessage("Access denied", Color.red);

            // Shake UI for access denied
            if (uiShake != null)
            {
                uiShake.Shake(accessDeniedShakeStrength, accessDeniedShakeDuration);
            }

            return;
        }

        // Notify tutorial system of navigation
        if (useTutorial && tutorialManager != null)
        {
            tutorialManager.OnNavigationOccurred();
        }

        StartCoroutine(AnimateTravel(currentNode, targetNode));
    }

    private bool TryConsumeAccess(Node targetNode)
    {
        // Enforce access restriction based on node type
        int requiredLevel = GetRequiredAccessForNodeType(targetNode.nodeType);
        if (accessLevel < requiredLevel)
        {
            Debug.Log($"Access level {accessLevel} too low for {targetNode.nodeType}. Required: {requiredLevel}");
            MessageLine.Instance.ShowMessage($"Access level {accessLevel} too low for {targetNode.nodeType}. Required: {requiredLevel}", Color.yellow);
            return false;
        }

        // Consume access level only if it's a special node AND we haven't visited it before
        if ((targetNode.nodeType == NodeType.Firewall || targetNode.nodeType == NodeType.Antivius) &&
            targetNode.nodeStatus == NodeStatus.Unvisited)
        {
            AccessLevel -= requiredLevel;
            AccessLevel = Mathf.Max(0, accessLevel);

            Debug.Log($"Access level consumed: -{requiredLevel}. Remaining: {accessLevel}");
            MessageLine.Instance.ShowMessage($"Access level consumed: {requiredLevel}. Remaining: {accessLevel}", Color.cyan);

            // Shake UI when access is consumed
            if (uiShake != null)
            {
                uiShake.Shake(accessConsumedShakeStrength, accessConsumedShakeDuration);
            }
        }
        else if ((targetNode.nodeType == NodeType.Firewall || targetNode.nodeType == NodeType.Antivius) &&
                 targetNode.nodeStatus == NodeStatus.Visited)
        {
            Debug.Log($"Already visited {targetNode.nodeType} node. No access consumed.");
            MessageLine.Instance.ShowMessage($"Already visited {targetNode.nodeType} node. No access consumed.", Color.green);
        }

        return true;
    }

    private IEnumerator AnimateTravel(Node from, Node to)
    {
        Image edgeToAnimate = GetEdgeTravelBetween(from, to);
        if (edgeToAnimate == null)
            yield break;

        // Disable all other travel edges on 'from' except the current one
        DisableOtherEdges(from, edgeToAnimate);

        // Disable from-node's standard edges too (needed for Start node edge visibility)
        DisableStandardEdges(from);

        // Reset the opposite travel edge on 'to'
        Image oppositeEdge = GetEdgeTravelBetween(to, from);
        if (oppositeEdge != null)
        {
            oppositeEdge.fillAmount = 0f;
            oppositeEdge.enabled = false;
        }

        // Only disable the specific standard edge on 'to' that would interfere with travel animation
        DisableSpecificStandardEdge(to, from);

        float duration = 0.3f;
        float elapsed = 0f;

        edgeToAnimate.fillAmount = 0f;
        edgeToAnimate.enabled = true;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            edgeToAnimate.fillAmount = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        edgeToAnimate.fillAmount = 1f;

        // Transition complete — update node visuals
        from.nodeCurrentVisual.SetActive(false);
        to.nodeCurrentVisual.SetActive(true);
        from.nodeStatus = NodeStatus.Visited;
        from.UpdateNodeVisuals();

        currentNode = to;

        if (to.nodeType == NodeType.End)
        {
            onReachedEndNode?.Invoke();
        }
    }


    private void DisableOtherEdges(Node node, Image exceptEdge)
    {
        var edges = new List<Image>
        {
            node.edgeLeftTravel,
            node.edgeRightTravel,
            node.edgeUpTravel,
            node.edgeDownTravel
        };

        foreach (var edge in edges)
        {
            if (edge != null && edge != exceptEdge && edge.enabled)
            {
                edge.enabled = false;
                edge.fillAmount = 0f;
            }
        }
    }

    private int GetRequiredAccessForNodeType(NodeType type)
    {
        switch (type)
        {
            case NodeType.Firewall:
                return 2;
            case NodeType.Antivius:
                return 2;
            case NodeType.Start:
            case NodeType.End:
                return 0; // Always accessible
            case NodeType.Normal:
            default:
                return 0;
        }
    }

    private void DisableStandardEdges(Node node)
    {
        if (node.edgeLeft != null) node.edgeLeft.SetActive(false);
        if (node.edgeRight != null) node.edgeRight.SetActive(false);
        if (node.edgeUp != null) node.edgeUp.SetActive(false);
        if (node.edgeDown != null) node.edgeDown.SetActive(false);
    }

    private void DisableSpecificStandardEdge(Node node, Node fromNode)
    {
        // Determine which direction we're coming from and disable only that specific edge
        if (fromNode == node.GetLeftNeighbor())
        {
            if (node.edgeLeft != null) node.edgeLeft.SetActive(false);
        }
        else if (fromNode == node.GetRightNeighbor())
        {
            if (node.edgeRight != null) node.edgeRight.SetActive(false);
        }
        else if (fromNode == node.GetTopNeighbor())
        {
            if (node.edgeUp != null) node.edgeUp.SetActive(false);
        }
        else if (fromNode == node.GetBottomNeighbor())
        {
            if (node.edgeDown != null) node.edgeDown.SetActive(false);
        }
    }

    private bool IsConnected(Node from, Node to)
    {
        return from.connectedNeighbors.Contains(to);
    }

    private Image GetEdgeTravelBetween(Node from, Node to)
    {
        if (to == from.GetLeftNeighbor())
            return from.edgeLeftTravel;
        if (to == from.GetRightNeighbor())
            return from.edgeRightTravel;
        if (to == from.GetTopNeighbor())
            return from.edgeUpTravel;
        if (to == from.GetBottomNeighbor())
            return from.edgeDownTravel;

        return null;
    }

    public void OnClick_AbortHack()
    {
        EndHack();
    }

    // Method for tutorial system to handle node clicks
    public void OnNodeClickedForTutorial(GameObject nodeObject)
    {
        if (useTutorial && tutorialManager != null)
        {
            tutorialManager.OnNodeClicked(nodeObject);
        }
    }

    private void EndHack()
    {
        isTimerRunning = false;
        StopAllCoroutines();
        Destroy(gameObject);
    }
}