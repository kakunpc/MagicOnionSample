using System;
using System.Collections.Generic;
using Grpc.Core;
using MagicOnion.Client;
using Protocol;
using UniRx;
using UniRx.Async;
using UnityEngine;
using UnityEngine.UI;

public class CountComponent : MonoBehaviour, IRoomHubReceiver
{
    private Channel _channel;
    private IRoomHub _roomHub;
    private readonly BoolReactiveProperty _isJoin = new BoolReactiveProperty(false);
    private List<string> _logs = new List<string>();
        
    [SerializeField]
    private Text logText;

    [SerializeField]
    private Button joinOrLeftButton;

    [SerializeField]
    private Text joinOrLeftText;
        
    [SerializeField]
    private Button addButton;
        
    // Start is called before the first frame update
    void Start()
    {
        //Client側のHubの初期化
        _channel = new Channel("localhost:12345", ChannelCredentials.Insecure);
        _roomHub = StreamingHubClient.Connect<IRoomHub, IRoomHubReceiver>(_channel, this);


        joinOrLeftButton.OnClickAsObservable().Subscribe(async _ =>
        {
            joinOrLeftButton.interactable = false;
            if (_isJoin.Value)
            {
                await CallLeft();
            }
            else
            {
                await CallJoin();
            }
            joinOrLeftButton.interactable = true;
        }).AddTo(this);

        _isJoin.Subscribe(value =>
        {
            joinOrLeftText.text = value ? "LEFT" : "JOIN";
            addButton.interactable = value;
        }).AddTo(this);
        addButton.OnClickAsObservable().Subscribe(async _ => { await _roomHub.AddCookieAsync(); }).AddTo(this);
    }

    async UniTask CallJoin()
    {
        var userName = Guid.NewGuid().ToString();
        await _roomHub.JoinAsync(userName);
        _isJoin.Value = true;
        var countValue = await _roomHub.GetCookieAsync();
        AddLog($"NowCookie: {countValue}");
        await UniTask.Delay(1000);
    }

    async UniTask CallLeft()
    {
        await _roomHub.LeaveAsync();
        _isJoin.Value = false;
        await UniTask.Delay(1000);
    }
        
        
    async void OnDestroy()
    {
        await _roomHub.DisposeAsync();
        await _channel.ShutdownAsync();
    }


    public void OnJoin(string joinName)
    {
        AddLog($"{joinName} is Join.");
    }

    public void OnLeave(string leftName)
    {
        AddLog($"{leftName} is Left.");
    }

    public void OnChangeCookieCount(int value)
    {
        AddLog($"CookieCountChange: {value}");
    }

    void AddLog(string value)
    {
        _logs.Add(value);
        while (_logs.Count > 50)
        {
            _logs.RemoveAt(0);
        }

        logText.text = string.Join("\n", _logs.ToArray());
    }
}
