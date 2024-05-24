using System.Collections;
using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;

public class RunModel : MonoBehaviour
{
    public MovingObject targetObject;
    public NNModel modelAsset;
    public float[] input;

    [SerializeField] private float[] curPredictValue;
    private List<float[]> dataList;
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
        var input = new Tensor(1, 1, 16, 5, rawInput);

        var output = worker.Execute(input).PeekOutput();

        curPredictValue = output.AsFloats();

        input.Dispose();
        output.Dispose();
    }

    private void OnDestroy()
    {
        worker.Dispose();
    }

    void OnMessageArrived(string msg)
    {
        //Debug.Log("Recive Message : " + msg);

        string[] text = msg.Split(' ');
        float[] data = new float[text.Length];

        for(int i = 0;i < text.Length;i++)
        {
            data[i] = float.Parse(text[i]);
        }

        //dataList.Add(data);

        //if (dataList.Count == 16)
        //{
        //    dataList.RemoveAt(0);

        //    float[] dataArray = new float[dataList.Count * 5];
        //    for (int i = 0;i < dataList.Count; i++)
        //    {
        //        for(int j = 0;j < 5; j++)
        //        {
        //            dataArray[i * 5 + j] = dataList[i][j];
        //        }
        //    }
        //    Predict(dataArray);
        //}
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
