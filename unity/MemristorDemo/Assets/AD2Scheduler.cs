using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using static AD2Scheduler;

public class AD2Scheduler
{
    public enum AD2Instructions
    {
        ReadSingle,
        ReadAll,
        ReadTryte, //Propose 12 trits (all Unicode symbols and room for custom symbols)
        WriteSingle,
        EraseAll,
    }

    public ConcurrentQueue<AD2Instruction> Queue = new ConcurrentQueue<AD2Instruction>();

    public bool IsActive { get; set; } = false;
    public bool IsProcessIdle { get; set; }

    public void Schedule(AD2Instruction instruction)
    {
       Queue.Enqueue(instruction);
    }

    public AD2Instruction Process()
    {
        try
        {
            Queue.TryDequeue(out AD2Instruction result);
            return result;
        }
        catch
        {
            return null;
        }
    }

    public void Clear()
    {
        Queue = new ConcurrentQueue<AD2Instruction>(); //swapping is cheaper :)
    }
}

public class AD2Instruction{
    public AD2Instructions Instruction { get; set; }

    public int Id { get; set; }

    public int State { get; set; }
    
    public AD2Instruction(AD2Instructions instruction)
    {
        Instruction = instruction;
    }

    public AD2Instruction(AD2Instructions instruction, int id, int state) //used for reading (groundtruth) and writing
    {
        Instruction = instruction;
        Id = id;
        State = state;
    }

    public AD2Instruction(AD2Instructions instruction, int id) //used for when there is no groundtruth
    {
        Instruction = instruction;
        Id = id;
    }
}

