using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomSaber;
using UnityEngine.Events;

public class EventTester : MonoBehaviour {

    List<EventManager> managers;
    List<ComboReachedEvent> comboNbEvents;
    List<EveryNthComboFilter> comboNthEvents;

    [HideInInspector] public int combo;

    private string _comboStr;

	void Start () {
        managers = new List<EventManager>(FindObjectsOfType<EventManager>());
        comboNbEvents = new List<ComboReachedEvent>(FindObjectsOfType<ComboReachedEvent>());
        comboNthEvents = new List<EveryNthComboFilter>(FindObjectsOfType<EveryNthComboFilter>());
    }

    private void OnGUI()
    {
        GUILayout.BeginVertical();
        Button("On Slice", OnSlice);
        Button("Combo Break", ComboBreak);
        Button("Multiplier Up", MultiplierUp);
        Button("Start Colliding", StartColliding);
        Button("Stop Colliding", StopColliding);
        Button("Level Start", LevelStart);
        Button("Level Fail", LevelFail);
        Button("Level Ended", LevelEnded);
        Button("BlueLightOn", BlueLightOn);
        Button("RedLightOn", RedLightOn);
        Button("TestCombo", TestCombo);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Combo: ");
        _comboStr = GUILayout.TextField(_comboStr);
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    private void Button(string text, Action action)
    {
        if (GUILayout.Button(text))
        {
            action();
        }
    }

    public void OnSlice()
    {
        foreach(EventManager manager in managers)
        {
            manager.OnSlice.Invoke();
        }
    }

    public void ComboBreak()
    {
        foreach (EventManager manager in managers)
        {
            manager.OnComboBreak.Invoke();
        }
    }

    public void MultiplierUp()
    {
        foreach (EventManager manager in managers)
        {
            manager.MultiplierUp.Invoke();
        }
    }

    public void StartColliding()
    {
        foreach (EventManager manager in managers)
        {
            manager.SaberStartColliding.Invoke();
        }
    }

    public void StopColliding()
    {
        foreach (EventManager manager in managers)
        {
            manager.SaberStopColliding.Invoke();
        }
    }

    public void LevelStart()
    {
        foreach (EventManager manager in managers)
        {
            manager.OnLevelStart.Invoke();
        }
    }

    public void LevelFail()
    {
        foreach (EventManager manager in managers)
        {
            manager.OnLevelFail.Invoke();
        }
    }

    public void LevelEnded()
    {
        foreach (EventManager manager in managers)
        {
            manager.OnLevelEnded.Invoke();
        }
    }

    public void BlueLightOn()
    {
        foreach (EventManager manager in managers)
        {
            manager.OnBlueLightOn.Invoke();
        }
    }

    public void RedLightOn()
    {
        foreach (EventManager manager in managers)
        {
            manager.OnRedLightOn.Invoke();
        }
    }

    public void TestCombo()
    {
        SetCombo(_comboStr);

        foreach (EventManager manager in managers)
        {
            manager.OnComboChanged.Invoke(combo);
        }

        foreach (ComboReachedEvent ev in comboNbEvents)
        {
            if (ev.ComboTarget == combo)
                ev.NthComboReached.Invoke();
        }

        foreach (EveryNthComboFilter ev in comboNthEvents)
        {
            if (ev.ComboStep == combo)
                ev.NthComboReached.Invoke();
        }
    }

    public void SetCombo(string input)
    {
        combo = int.Parse(input);
    }
}
