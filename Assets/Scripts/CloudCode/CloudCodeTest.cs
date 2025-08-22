using System.Collections.Generic;
using Unity.Services.CloudCode;
using Unity.Services.Core;
using UnityEngine;

public class CloudCodeTest : MonoBehaviour
{
    private class ResultType
    {
        public string caca;
        public int Roll;
        public int Sides;
    }

    public async void HELLO()
    {
        try
        {
            Debug.Log("Calling");
            // Call the function within the module and provide the parameters we defined in there
            //var result = await CloudCodeService.Instance.CallModuleEndpointAsync<string>("LifeMergeCloudCodeddd", "SayHello");
            
            
            var args = new Dictionary<string, object>();
            args.Add("paramtest1", "hola");
            var result = await CloudCodeService.Instance.CallEndpointAsync<ResultType>("test",args);
            
            
            Debug.Log(result);
        }
        catch (CloudCodeException exception)
        {
            Debug.Log("Ha Fallado");
            Debug.Log(exception.Reason);
        }
    }
}
