using System.Collections;
using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;

public class RunModel : MonoBehaviour
{
    public MovingObject targetObject;
    public NNModel modelAsset;
    public float[] input;

    private Model runTimeModel;
    private IWorker worker;

    private void Start()
    {
        runTimeModel = ModelLoader.Load(modelAsset);
        worker = WorkerFactory.CreateWorker(runTimeModel, WorkerFactory.Device.CPU);
        Predict(input);
    }

    public void Predict(float[] rawInput)
    {
        var input = new Tensor(1, 1, 5, 16, rawInput);

        var output = worker.Execute(input).PeekOutput();

        float[] data = output.AsFloats();

        string res = "";

        foreach (float d in data)
        {
            res += d.ToString() + " ";
        }

        Debug.Log(res);

        input.Dispose();
        output.Dispose();
    }

    private void OnDestroy()
    {
        worker.Dispose();
    }

    void OnMessageArrived(string msg)
    {
        Debug.Log("Recive Message : " + msg);

        string[] text = msg.Split(' ');
        float[] data = new float[text.Length];

        for(int i = 0;i < text.Length;i++)
        {
            data[i] = float.Parse(text[i]);
        }

        Predict(data);
    }

    void OnConnectionEvent(bool success)
    {
        if(success)
        {
            Debug.Log("Connect Arduino");
        }
        else
        {
            Debug.Log("DisConnect Arduino");
        }
    }
}
