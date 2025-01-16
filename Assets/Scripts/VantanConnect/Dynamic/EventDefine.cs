
//これはコードで自動生成されます
//みんなが使用できるイベントのみが表示されます
namespace VTNConnect
{
    public enum EventDefine
    {
        DemonUI = 101,  //岩垂UIの憑依 (BossBattle2D -> すべてのゲーム、対象のみ)
        JengaInfo = 104,  //ジェンガ1本抜くと情報がもらえる (Jenga -> VCMain、対象のみ)
        ActorEffect = 105,  //アクターにランダム効果 (すべてのゲーム -> Confront、対象のみ)
        ReviveGimmick = 106,  //ステージギミック復活(罠系) (すべてのゲーム -> Confront、対象のみ)
        DarkRoom = 107,  //照明の光度が一定時間低下する。 (すべてのゲーム -> Confront、対象のみ)
        SummonEnemy = 109,  //プレイヤーの頭上から雑魚敵が降ってくる (すべてのゲーム -> Confront、対象のみ)
        BadJengaInfo = 112,  //logを流して何か起こる(悪影響) (Jenga -> VCMain、対象のみ)
        Cheer = 1001,  //おうえんメッセージ (バンコネシステム -> すべてのゲーム、対象のみ)
        BonusCoin = 1002,  //コイン増える (バンコネシステム -> すべてのゲーム、自分のみ)

    }
}
