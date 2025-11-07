public interface IMenu
{
    void Setup(UIMainManager mngr);

    void Show();

    void Show(GameManager.eLevelMode mode);

    void Hide();
}
