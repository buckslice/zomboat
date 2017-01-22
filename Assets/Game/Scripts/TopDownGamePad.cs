using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using HappyFunTimes;

// Manages the connection between this object and the phone
public class TopDownGamePad : MonoBehaviour {
    private NetPlayer netPlayer;

    public string playerName;
    private int charNumb;
    public bool touching = false;
    public Vector2 dir;
    Quaternion targetRot;

    public Color color;
    Color oldColor;

    static int s_colorCount = 0;    // so diff players have diff colors

    public event Action OnDeath;
    public event Action OnDisconnect;
    public event Action<Color> OnColorChanged;
    public event Action OnTap;

    public event EventHandler<EventArgs> OnNameChange;

    HFTPlayerNameManager playerNameManager;
    float timeSinceTouched = 100.0f;
    const float tapTouchThreshold = 0.2f;

    const int angleIntervals = 32;  // make sure this is same in controller js
    // angles represents counterclockwise angle from 0-31 starting at the right
    private class MessageTouchDir {
        public int angle = 0;
    }

    private class MessageTouch {
        public bool touching = false;
        public int angle = 0;
    }

    private class MessageColor {
        public MessageColor(Color _color) {
            color = _color;
        }
        public Color color;
    }

    private class MessageNumber {
        public MessageNumber(int number) {
            this.number = number;
        }
        public int number;
    }

    void Awake() {
        PickRandomColor();
    }

    void PickRandomColor() {
        int colorNdx = s_colorCount++;

        // Pick a color
        float hue = (((colorNdx & 0x01) << 5) |
                     ((colorNdx & 0x02) << 3) |
                     ((colorNdx & 0x04) << 1) |
                     ((colorNdx & 0x08) >> 1) |
                     ((colorNdx & 0x10) >> 3) |
                     ((colorNdx & 0x20) >> 5)) / 64.0f;
        float sat = (colorNdx & 0x10) != 0 ? 0.5f : 1.0f;
        float value = (colorNdx & 0x20) != 0 ? 0.5f : 1.0f;
        float alpha = 1.0f;

        Vector4 hsva = new Vector4(hue, sat, value, alpha);
        color = HFTColorUtils.HSVAToColor(hsva);
    }

    public void InitializeNetPlayer(NetPlayer netPlayer) {
        this.netPlayer = netPlayer;


        netPlayer.OnDisconnect += HandleDisconnect;

        // Setup events for the different messages.
        netPlayer.RegisterCmdHandler<MessageTouch>("touch", HandleTouch);
        netPlayer.RegisterCmdHandler<MessageTouchDir>("touchDir", HandleTouchDir);

        playerNameManager = new HFTPlayerNameManager(netPlayer);
        playerNameManager.OnNameChange += HandleNameChange;

        // send play command
        netPlayer.SendCmd("play");
        SendImage();
        SendColor();
    }

    void HandleTouch(MessageTouch data) {
        touching = data.touching;
        if (touching) {
            SetDir(data.angle);
            timeSinceTouched = 0.0f;
        } else if (timeSinceTouched < tapTouchThreshold) {
            if (OnTap != null) {
                OnTap();    // do action move
            }
            timeSinceTouched = 100.0f;  // make sure no double action
        }
        //Debug.Log("TOUCH EVENT " + Time.time);
    }

    void HandleTouchDir(MessageTouchDir data) {
        //Debug.Log(data.angle);
        SetDir(data.angle);
    }

    void SetDir(int angle) {
        Quaternion q = Quaternion.AngleAxis(angle * 360.0f / angleIntervals, Vector3.forward);
        dir = q * Vector2.right;
        targetRot = q;
    }

    void OnDestroy() {
        netPlayer.OnDisconnect -= HandleDisconnect;
        if (playerNameManager != null) {
            playerNameManager.Close();
            playerNameManager = null;
        }
    }

    void HandleDisconnect(object sender, EventArgs e) {
        if (OnDisconnect != null) {
            OnDisconnect();
        }
    }

    void HandleNameChange(string name) {
        playerName = name;
        EventHandler<EventArgs> handler = OnNameChange;
        if (handler != null) {
            handler(this, new EventArgs());
        }
    }

    // Update is called once per frame
    void Update() {
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, 0.1f);
        timeSinceTouched += Time.deltaTime;
        if (oldColor != color) {
            oldColor = color;
            SendColor();
        }
    }

    void SendColor() {
        if (netPlayer != null) {
            netPlayer.SendCmd("color", new MessageColor(color));

            if (OnColorChanged != null) {
                OnColorChanged(color);
            }
        }
    }

    void SendImage() {
        if (netPlayer != null) {
            charNumb = UnityEngine.Random.Range(1, 19);
            netPlayer.SendCmd("picture", new MessageNumber(charNumb));
        }
    }

    public void ChangeLives(int newHealth, int oldHealth) {
        Debug.Log("ChangeLive " + newHealth + " " + oldHealth);
        if (newHealth < oldHealth && UnityEngine.Random.Range(0.0f, 100.0f) >= (100.0f - GameManager.instance.chompFrequency)) {
            SoundManager.instance.PlaySound(1);
        }

        if (netPlayer != null) {
            netPlayer.SendCmd("livechange", new MessageNumber(newHealth));
        }
    }

    public void SendZombification() {
        netPlayer.SendCmd("zomb", new MessageNumber(charNumb));
    }

    public void SendRole(Role role) {
        netPlayer.SendCmd("role", role.ToString().ToLower());
    }


}
