public static class MultiplayResetter
{
    public static void ResetAll() {
        // 1. 네트워크 상태 정리
        if (LiteNetLibManager.Instance != null) {
            UnityEngine.Object.Destroy(LiteNetLibManager.Instance.gameObject);
        }

        // 2. 랭킹 UI 초기화
        if (RankingUIManager.Instance != null) {
            UnityEngine.Object.Destroy(RankingUIManager.Instance.gameObject);
        }
    }
}
