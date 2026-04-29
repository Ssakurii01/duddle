using Luxodd.Game.Scripts.Network;
using System;
using System.Collections;
using System.Collections.Generic;
using Luxodd.Game.Scripts.HelpersAndUtils.Logger;
using UnityEngine;

public class CommandQueueInspector : MonoBehaviour
{
    private Dictionary<CommandRequestType, int> _enqueueCounts = new Dictionary<CommandRequestType, int>();
    private Dictionary<CommandRequestType, int> _dequeueCounts = new Dictionary<CommandRequestType, int>();
    private Coroutine _checkRoutine;

    private void Start()
    {
        _checkRoutine = StartCoroutine(PeriodicCheck());
    }

    private IEnumerator PeriodicCheck()
    {
        while (true)
        {
            yield return new WaitForSeconds(15f);
            CheckMismatches();
        }
    }
    public void Enqueue(CommandRequestType type)
    {
        if (!_enqueueCounts.ContainsKey(type))
            _enqueueCounts[type] = 0;

        _enqueueCounts[type]++;
        LoggerHelper.LogWarning($"[CommandQueueInspector] Enqueued: {type}, Total Enqueued: {_enqueueCounts[type]}");
    }

    public void Dequeue(CommandRequestType type)
    {
        if (!_dequeueCounts.ContainsKey(type))
            _dequeueCounts[type] = 0;

        _dequeueCounts[type]++;
        LoggerHelper.LogWarning($"[CommandQueueInspector] Dequeued: {type}, Total Dequeued: {_dequeueCounts[type]}");
    }

    public void CheckMismatches()
    {
        var allKeys = new HashSet<CommandRequestType>(_enqueueCounts.Keys);
        allKeys.UnionWith(_dequeueCounts.Keys);

        foreach (var type in allKeys)
        {
            _enqueueCounts.TryGetValue(type, out int enq);
            _dequeueCounts.TryGetValue(type, out int deq);

            if (enq > deq)
            {
                LoggerHelper.LogWarning($"❗ Command '{type}' was enqueued {enq} times but dequeued only {deq} times.");
            }
            else if (deq > enq)
            {
                LoggerHelper.LogWarning($"❗ Command '{type}' was dequeued {deq} times but enqueued only {enq} times.");
            }
            else
            {
                LoggerHelper.LogWarning($"✅ Command '{type}' was properly enqueued and dequeued ({enq} times).");
            }
        }

        LoggerHelper.LogWarning("[CommandQueueInspector] Command check complete.");
    }

    public void ResetInspector()
    {
        _enqueueCounts.Clear();
        _dequeueCounts.Clear();
    }
}
