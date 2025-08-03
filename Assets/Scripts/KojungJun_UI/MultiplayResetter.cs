public static class MultiplayResetter
{
    public static void ResetAll() {
        // 1. ��Ʈ��ũ ���� ����
        if (LiteNetLibManager.Instance != null) {
            UnityEngine.Object.Destroy(LiteNetLibManager.Instance.gameObject);
        }

        // 2. ��ŷ UI �ʱ�ȭ
        if (RankingUIManager.Instance != null) {
            UnityEngine.Object.Destroy(RankingUIManager.Instance.gameObject);
        }
    }
}
