using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class BookUIController : MonoBehaviour
{
    [Header("Book Setup")]
    public GameObject bookUI;
    public Animator bookAnimator;
    public Animator contentAnimator;

    [Header("Tab Pages")]
    public GameObject[] pages;
    public GameObject tabsContainer;

    public static bool BookIsOpen { get; private set; }

    private bool isFirstOpen = true;
    private bool bookOpen = false;
    private int currentPage = 0;
    private int pendingPageIndex = 0;

    private float bookOpenDelay = 0.5f;
    private float tabAppearDelay = 0.4f;
    private float tabDisappearDelay = 0.4f;
    private float bookCloseDelay = 0.5f;
    private float pageFlipDelay = 0.4f;
    private float contentTransitionDelay = 0.92f;

    void Update()
    {
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            Debug.Log($"[BookUI] F key pressed. Book is currently {(bookOpen ? "OPEN" : "CLOSED")}");
            if (bookOpen)
                CloseBook();
            else
                OpenBook(0);
        }
    }

    public void OpenBook(int pageIndex)
    {
        // ? Reset to page 0 if something passed a bad index
        if (pageIndex < 0 || pageIndex >= pages.Length)
            pageIndex = 0;

        currentPage = 0;
        pendingPageIndex = pageIndex;

        if (PauseManager.Instance != null && PauseManager.Instance.CurrentPauseType == PauseType.None)
        {
            PauseManager.Instance.Pause(PauseType.Combat);
            Debug.Log("[BookUI] Game logic paused (soft) due to book opening.");
        }

        bookUI.SetActive(true);
        bookAnimator.Play("BookOpen1");
        bookOpen = true;
        BookIsOpen = true;

        Invoke(nameof(PlayTabAppear), bookOpenDelay);
        Invoke(nameof(ShowTabs), bookOpenDelay + tabAppearDelay);
        Invoke(nameof(FinishOpenBook), bookOpenDelay + tabAppearDelay + 0.1f);
    }

    public void CloseBook()
    {
        // ? Resume soft pause if caused by book
        if (PauseManager.Instance != null && PauseManager.Instance.CurrentPauseType == PauseType.Combat)
        {
            PauseManager.Instance.Resume();
            Debug.Log("[BookUI] Game logic resumed after closing book.");
        }

        // ? Reset page state immediately
        currentPage = 0;
        pendingPageIndex = 0;

        // ? Hide all pages
        foreach (var page in pages)
            page.SetActive(false);

        HideTabs();

        // ? Run close animations
        bookAnimator.Play("TabDissapearNI1");
        Invoke(nameof(PlayBookClose), tabDisappearDelay);
        Invoke(nameof(HideAll), tabDisappearDelay + bookCloseDelay);

        bookOpen = false;
        BookIsOpen = false;
    }

    IEnumerator DelayInitialPageDisplay()
    {
        contentAnimator.gameObject.SetActive(true);
        contentAnimator.Play("ContentAppear1");

        yield return new WaitForSecondsRealtime(contentTransitionDelay);

        SwitchToPage(pendingPageIndex);

        contentAnimator.gameObject.SetActive(false);
    }

    void PlayTabAppear()
    {
        bookAnimator.Play("TabAppearNI1");
    }

    void ShowTabs()
    {
        tabsContainer.SetActive(true);
    }

    void FinishOpenBook()
    {
        if (isFirstOpen)
        {
            isFirstOpen = false;
            StartCoroutine(DelayInitialPageDisplay());
        }
        else
        {
            SwitchToPage(pendingPageIndex);
        }
    }

    void HideTabs()
    {
        tabsContainer.SetActive(false);
    }

    void PlayBookClose()
    {
        bookAnimator.Play("BookClose2");
    }

    void HideAll()
    {
        bookUI.SetActive(false);
        foreach (var page in pages)
            page.SetActive(false);
    }

    public void OnClickTab(int newIndex)
    {
        if (!bookOpen || newIndex == currentPage)
            return;

        StartCoroutine(SwitchPageWithTransition(newIndex));
    }

    IEnumerator SwitchPageWithTransition(int newPage)
    {
        contentAnimator.gameObject.SetActive(true);
        contentAnimator.Play("ContentAppear1");
        yield return new WaitForSecondsRealtime(contentTransitionDelay);

        bookAnimator.Play("TabDissapearNI1");
        yield return new WaitForSecondsRealtime(tabDisappearDelay);

        tabsContainer.SetActive(false);

        foreach (var page in pages)
            page.SetActive(false);

        if (newPage > currentPage)
            bookAnimator.Play("PageFlipR1");
        else
            bookAnimator.Play("PageFlipL1");

        yield return new WaitForSecondsRealtime(pageFlipDelay);

        bookAnimator.Play("TabAppearNI1");
        yield return new WaitForSecondsRealtime(tabAppearDelay);
        tabsContainer.SetActive(true);

        contentAnimator.Play("ContentDissapear1");
        yield return new WaitForSecondsRealtime(contentTransitionDelay);

        pages[newPage].SetActive(true);
        contentAnimator.gameObject.SetActive(false);
        currentPage = newPage;
    }

    void SwitchToPage(int index)
    {
        for (int i = 0; i < pages.Length; i++)
            pages[i].SetActive(i == index);
    }
}