using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.OsuManiaLoader;

public class ManiaToInvaxion
{
    private static readonly int[,] KeyMap = {
        { 11, 12, 15, 16, 00, 00, 00, 00 },
        { 27, 11, 12, 15, 16, 29, 00, 00 },
        { 27, 11, 12, 13, 14, 15, 16, 29 }
    };
    private int _keyMode;
    private int _keyNum;
    private int _bpm;
    private float _oneBeatTime;
    private int _offset;
    private int _beatDivisor;
    private float _oneDivisorTime;
    private int _columnWidth;
    private List<TmpNote> _tmpNotes = new();
    private Dictionary<int, InvaxionBar> _invaxionMap = new();
    private Dictionary<float, TmpTimeline> _timeLine = new();
    private StringBuilder _invaxionMapStr = new();
    private OsuMania _beatmap;

    public ManiaToInvaxion(OsuMania beatmap)
    {
        _beatmap = beatmap;
        _keyMode = beatmap.KeyCount switch
        {
            6 => 1,
            8 => 2,
            _ => 0
        };
        _keyNum = _keyMode switch
        {
            1 => 6,
            2 => 8,
            _ => 4
        };
        _columnWidth = 512 / _keyNum;
        _oneBeatTime = beatmap.TimingPoints[0].MsPerBeat;
        _offset = beatmap.TimingPoints[0].Time;
        _beatDivisor = beatmap.BeatDivisor;
        _oneDivisorTime = _oneBeatTime / _beatDivisor;
        _bpm = (int)(60 * 1000 / _oneBeatTime);
    }

    public void Convert(out string map, out int audioFill)
    {
        _tmpNotes.Clear();
        _timeLine.Clear();
        _invaxionMap.Clear();
        
        var fillNode = (int)(_offset / _oneDivisorTime);
        var fillTime = _oneDivisorTime - (_offset % _oneDivisorTime);
        
        if (fillTime > 0)
        {
            fillNode++;
        }

        foreach (var i in _beatmap.HitObjects)
        {
            var startTime = i.Time - _offset;
            var endTime = i is OsuManiaLongNote l ? l.EndTime - _offset : startTime;
            var barIndex = 0;
            var nodeIndex = 0;
            
            CalcIndex(startTime, fillNode, out barIndex, out nodeIndex);
            if (i is OsuManiaLongNote)
            {
                _tmpNotes.Add(new TmpNote
                {
                    Key = X2Key(i.x),
                    Action = 11,
                    Time = startTime,
                    BarIndex = barIndex,
                    NodeIndex = nodeIndex,
                });
            }
            else
            {
                var k = X2Key(i.x);
                _tmpNotes.Add(new TmpNote
                {
                    Key = k,
                    Action = 31,
                    Time = startTime,
                    BarIndex = barIndex,
                    NodeIndex = nodeIndex
                });
                CalcIndex(endTime, fillNode, out barIndex, out nodeIndex);
                _tmpNotes.Add(new TmpNote
                {
                    Key = k,
                    Action = 41,
                    Time = endTime,
                    BarIndex = barIndex,
                    NodeIndex = nodeIndex
                });
            }
        }
        
        foreach (var i in _tmpNotes)
        {
            if (!_invaxionMap.ContainsKey(i.BarIndex))
            {
                _invaxionMap.Add(i.BarIndex, new InvaxionBar()
                {
                    Tracks = new Dictionary<int, InvaxionTrack>()
                });
            }
            if (!_invaxionMap[i.BarIndex].Tracks.ContainsKey(i.Key))
            {
                _invaxionMap[i.BarIndex].Tracks.Add(i.Key, new InvaxionTrack()
                {
                    Nodes = new Dictionary<int, InvaxionNode>()
                });
            }
            if (!_invaxionMap[i.BarIndex].Tracks[i.Key].Nodes.ContainsKey(i.NodeIndex))
            {
                _invaxionMap[i.BarIndex].Tracks[i.Key].Nodes.Add(i.NodeIndex, new InvaxionNode()
                {
                    Action = i.Action
                });
            }
        }
        
        if(!_invaxionMap.ContainsKey(0))
        {
            _invaxionMap.Add(0, null);
        }
        
        _invaxionMap = _invaxionMap.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);
        foreach (var i in _invaxionMap)
        {
            // 准备
            if (i.Key == 0)
            {
                _invaxionMapStr.AppendFormat("0:\n1,{0};\n\n", _bpm);
            }
            // Bar
            _invaxionMapStr.AppendFormat("{0}:\n", i.Key + 1);
            // Start
            if (i.Key == 0)
            {
                _invaxionMapStr.Append("3,1,\n");
            }
            // Track
            if (i.Value != null)
            {
                i.Value.Tracks = i.Value.Tracks.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);
                foreach (var j in i.Value.Tracks)
                {
                    _invaxionMapStr.AppendFormat("{0},", j.Key);
                    for (var k = 0; k < _beatDivisor * 4; k++)
                    {
                        _invaxionMapStr.Append(j.Value.Nodes.ContainsKey(k) ? j.Value.Nodes[k].Action.ToString() : "00");
                    }
                    _invaxionMapStr.AppendFormat(",\n");
                }
            }
            _invaxionMapStr.Remove(_invaxionMapStr.Length - 2, 2);
            _invaxionMapStr.Append(";\n\n");
        }
        map = _invaxionMapStr.ToString();
        audioFill = (int)Math.Round(fillTime);
    }
    
    private void CalcIndex(int time, int fill, out int barIndex, out int nodeIndex)
    {
        int oneBarNode = _beatDivisor * 4;
        int divisorNum = (int)Math.Round(time / _oneDivisorTime);
        barIndex = divisorNum / oneBarNode;
        nodeIndex = divisorNum % oneBarNode;

        nodeIndex += fill;
        barIndex += nodeIndex / oneBarNode;
        nodeIndex %= oneBarNode;
    }

    private int X2Key(int x)
    {
        return KeyMap[_keyMode, x / _columnWidth];
    }
}