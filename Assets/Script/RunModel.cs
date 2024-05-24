using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.Barracuda;
using UnityEngine;

public class RunModel : MonoBehaviour
{
    public MovingObject targetObject;
    public NNModel modelAsset;
    public float[] input;

    [SerializeField] private float[] curPredictValue;
    private List<float[]> dataList = new List<float[]>();
    private List<float[]> truth = new List<float[]>();
    private Model runTimeModel;
    private IWorker worker;

    private void Start()
    {
        runTimeModel = ModelLoader.Load(modelAsset);
        worker = WorkerFactory.CreateWorker(runTimeModel, WorkerFactory.Device.CPU);
    }

    public void Predict(float[] rawInput)
    {
        var input = new Tensor(1, 1, 16, 5, rawInput);

        var output = worker.Execute(input).PeekOutput();

        curPredictValue = output.AsFloats();

        int state = GetMaxIndex(curPredictValue);

        targetObject.ChangeMove((MoveState)state);

        input.Dispose();
        output.Dispose();
    }

    private int GetMaxIndex(float[] array)
    {
        float max = array.Max();

        for (int i = 0;i < array.Length;i++)
        {
            if (max == array[i])
                return i;
        }

        return -1;
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

        float[] mean = { 0.44578253f, 0.73138337f, 0.46890912f, 0.37729042f, 0.26794667f };
        float[] std = { 0.5255335f,  0.7421981f,  0.41341906f, 0.41430854f, 0.36469f };
        for (int i = 0;i < text.Length;i++)
        {
            data[i] = float.Parse(text[i]);
            data[i] = (data[i] - mean[i]) / std[i];
        }

        dataList.Add(data);

        if (dataList.Count == 16)
        {
            float[] dataArray = new float[dataList.Count * 5];
            for (int i = 0; i < dataList.Count; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    dataArray[i * 5 + j] = dataList[i][j];
                }
            }
            Predict(dataArray);

            dataList.RemoveAt(0);
        }
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
