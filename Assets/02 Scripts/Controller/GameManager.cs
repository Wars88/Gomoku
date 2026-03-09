using UnityEngine;
using UnityEngine.SceneManagement;
using static Won.Constants;

namespace Won
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public StoneType MyStoneColor { get; private set; }
        public GameMode CurrentGameMode { get; private set; }
        public PlayerProfile MyProfile { get; private set; }
        public GameResult LastResult { get; private set; }

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 임시 프로필 — 나중에 로그인 정보로 교체
            MyProfile = new PlayerProfile { DisplayName = "플레이어", UserId = "local" };
        }

        public void StartGame(GameMode mode, StoneType myStone = StoneType.White)
        {
            CurrentGameMode = mode;
            MyStoneColor = myStone;
            SceneManager.LoadScene("Game");
        }

        public void SaveResult(GameResult result)
        {
            LastResult = result;
            // TODO: PlayerPrefs 또는 서버 저장
        }

        public void GotoMainMenu() => SceneManager.LoadScene("Main");
    }
}