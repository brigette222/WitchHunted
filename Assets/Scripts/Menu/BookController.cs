using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class BookUIController : MonoBehaviour
{
    [Header("Book Setup")]
    public GameObject bookUI; // Root UI object for the book
    public Animator bookAnimator; // Animator controlling book animations
    public Animator contentAnimator; // Animator controlling page content transitions

    [Header("Tab Pages")]
    public GameObject[] pages; // Array of book pages
    public GameObject tabsContainer; // Container holding tab buttons

    public static bool BookIsOpen { get; private set; } // Tracks whether the book is open

    private bool isFirstOpen = true; // Tracks whether this is the first time opening
    private bool bookOpen = false; // Tracks current open/closed state
    private int currentPage = 0; // Currently displayed page
    private int pendingPageIndex = 0; // Next page to display after animations

    private float bookOpenDelay = 0.5f; // Delay before opening finishes
    private float tabAppearDelay = 0.4f; // Delay for tab appearance
    private float tabDisappearDelay = 0.4f; // Delay for tab disappearance
    private float bookCloseDelay = 0.5f; // Delay before closing finishes
    private float pageFlipDelay = 0.4f; // Delay for page flip animation
    private float contentTransitionDelay = 0.92f; // Delay for content fade animations

    void Update() // Called every frame
    {
        if (Keyboard.current.fKey.wasPressedThisFrame) // Check if F key pressed
        {
            if (bookOpen) CloseBook(); // If book open, close it
            else OpenBook(0); // Otherwise open to first page
        }
    }

    public void OpenBook(int pageIndex) // Opens the book at a specific page
    {
        if (pageIndex < 0 || pageIndex >= pages.Length) pageIndex = 0; // Clamp page index
        currentPage = 0; // Reset page
        pendingPageIndex = pageIndex; // Store requested page

        if (PauseManager.Instance != null && PauseManager.Instance.CurrentPauseType == PauseType.None) // Pause gameplay
            PauseManager.Instance.Pause(PauseType.Combat);

        bookUI.SetActive(true); // Show book UI
        bookAnimator.Play("BookOpen1"); // Play book open animation
        bookOpen = true; // Mark book as open
        BookIsOpen = true; // Update static state

        Invoke(nameof(PlayTabAppear), bookOpenDelay); // Schedule tab animation
        Invoke(nameof(ShowTabs), bookOpenDelay + tabAppearDelay); // Schedule tab visibility
        Invoke(nameof(FinishOpenBook), bookOpenDelay + tabAppearDelay + 0.1f); // Schedule page switch
    }

    public void CloseBook() // Closes the book
    {
        if (PauseManager.Instance != null && PauseManager.Instance.CurrentPauseType == PauseType.Combat) // Resume gameplay
            PauseManager.Instance.Resume();

        currentPage = 0; // Reset current page
        pendingPageIndex = 0; // Reset pending page
        foreach (var page in pages) page.SetActive(false); // Hide all pages
        HideTabs(); // Hide tabs

        bookAnimator.Play("TabDissapearNI1"); // Play tab disappear animation
        Invoke(nameof(PlayBookClose), tabDisappearDelay); // Schedule book close
        Invoke(nameof(HideAll), tabDisappearDelay + bookCloseDelay); // Schedule UI hide

        bookOpen = false; // Mark as closed
        BookIsOpen = false; // Update static state
    }

    IEnumerator DelayInitialPageDisplay() // Displays first page with delay
    {
        contentAnimator.gameObject.SetActive(true); // Show content animator
        contentAnimator.Play("ContentAppear1"); // Play appear animation
        yield return new WaitForSecondsRealtime(contentTransitionDelay); // Wait
        SwitchToPage(pendingPageIndex); // Switch to pending page
        contentAnimator.gameObject.SetActive(false); // Hide content animator
    }

    void PlayTabAppear() => bookAnimator.Play("TabAppearNI1"); // Play tab appear animation
    void ShowTabs() => tabsContainer.SetActive(true); // Enable tab container
    void HideTabs() => tabsContainer.SetActive(false); // Disable tab container
    void PlayBookClose() => bookAnimator.Play("BookClose2"); // Play book close animation

    void FinishOpenBook() // Called after book open sequence
    {
        if (isFirstOpen) // If first time opening
        {
            isFirstOpen = false; // Mark as opened
            StartCoroutine(DelayInitialPageDisplay()); // Delay before showing first page
        }
        else SwitchToPage(pendingPageIndex); // Otherwise switch instantly
    }

    void HideAll() // Hides book UI and pages
    {
        bookUI.SetActive(false); // Hide root book UI
        foreach (var page in pages) page.SetActive(false); // Hide all pages
    }

    public void OnClickTab(int newIndex) // Called when clicking a tab
    {
        if (!bookOpen || newIndex == currentPage) return; // Skip if invalid or same page
        StartCoroutine(SwitchPageWithTransition(newIndex)); // Transition to new page
    }

    IEnumerator SwitchPageWithTransition(int newPage) // Handles tab/page transitions
    {
        contentAnimator.gameObject.SetActive(true); // Show content animator
        contentAnimator.Play("ContentAppear1"); // Play appear animation
        yield return new WaitForSecondsRealtime(contentTransitionDelay); // Wait

        bookAnimator.Play("TabDissapearNI1"); // Play tab disappear animation
        yield return new WaitForSecondsRealtime(tabDisappearDelay); // Wait

        tabsContainer.SetActive(false); // Hide tab container
        foreach (var page in pages) page.SetActive(false); // Hide all pages

        if (newPage > currentPage) bookAnimator.Play("PageFlipR1"); // Play right flip animation
        else bookAnimator.Play("PageFlipL1"); // Play left flip animation

        yield return new WaitForSecondsRealtime(pageFlipDelay); // Wait for flip

        bookAnimator.Play("TabAppearNI1"); // Play tab reappear animation
        yield return new WaitForSecondsRealtime(tabAppearDelay); // Wait
        tabsContainer.SetActive(true); // Show tab container

        contentAnimator.Play("ContentDissapear1"); // Play content disappear animation
        yield return new WaitForSecondsRealtime(contentTransitionDelay); // Wait

        pages[newPage].SetActive(true); // Show new page
        contentAnimator.gameObject.SetActive(false); // Hide animator
        currentPage = newPage; // Update current page
    }

    void SwitchToPage(int index) // Switch instantly to a specific page
    {
        for (int i = 0; i < pages.Length; i++) pages[i].SetActive(i == index); // Enable only target page
    }
}