using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTeam : MonoBehaviourPun
{
    [System.Serializable]
    public class TeamButtonData
    {
        public Button button;
        public string teamName;
    }

    [Header("Team Buttons")]
    public TeamButtonData[] teamButtons;

    [Header("CURRENT TEAM (empty = no team)")]
    [SerializeField] private string currentTeam = "";

    public string CurrentTeam => currentTeam;

    private void Awake()
    {
        for (int i = 0; i < teamButtons.Length; i++)
        {
            int index = i;

            if (teamButtons[index].button != null)
            {
                teamButtons[index].button.onClick.AddListener(() =>
                {
                    SetTeam(teamButtons[index].teamName);
                });
            }
        }
    }

    private void SetTeam(string teamName)
    {
        if (!photonView.IsMine) return;

        ApplyTeam(teamName);

        photonView.RPC(
            nameof(ApplyTeamRPC),
            RpcTarget.AllBuffered,
            teamName
        );
    }

    [PunRPC]
    private void ApplyTeamRPC(string teamName)
    {
        ApplyTeam(teamName);
    }

    private void ApplyTeam(string teamName)
    {
        currentTeam = teamName; // "" = no team
    }
}