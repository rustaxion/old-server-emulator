using SevenZip;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Server.Networking;

public class Request : MonoBehaviour, ICodeProgress
{
    public Request(string url)
    {
        this.url = url;
    }
    public Request(string url, Dictionary<string, string> Headers)
    {
        this.url = url;
        headers = Headers;
    }

    public Request(string url, string method)
    {
        this.url = url;
        headers = new()
        {
            {"method", method}
        };
    }

    public Request(string url, string method, Dictionary<string, string> Headers)
    {
        this.url = url;
        headers = Headers;
        headers["method"] = method;
    }

    public Request(string url, string method, byte[] postData)
    {
        this.url = url;
        headers = new()
        {
            {"method", method}
        };
        this.postData = postData;
    }

    public Request(string url, string method, byte[] postData, Dictionary<string, string> Headers)
    {
        this.url = url;
        headers = Headers;
        headers["method"] = method;
        this.postData = postData;
    }

    public float GetCurrentProgress()
    {
        if (!successflag)
        {
            return 0f;
        }

        if (_www == null) return 0.5f;

        if (_www.isDone)
        {
            return 0.7f + progress * 0.3f;
        }
        else
        {
            return _www.progress * 0.7f;
        }
    }

    public void SetProgress(long a, long b)
    {
        progress = a / b;
    }

    public IEnumerator Download(onCompleted proc)
    {
        onCompletedProc = proc;
        progress = 0f;
        return downloadprogress();
    }

    public IEnumerator ReDownload()
    {
        return downloadprogress();
    }

    private IEnumerator downloadprogress()
    {
        yield return new WaitForFixedUpdate();
        _www = new WWW(url, postData, headers);

        successflag = true;
        yield return _www;
        bool isSuccess = false;
        if (string.IsNullOrEmpty(_www.error) && _www.isDone)
        {
            isSuccess = true;
        }
        else
        {
            Debug.LogError("Download Failed,Log: " + _www.error);
        }

        successflag = isSuccess;
        onCompletedProc(this, isSuccess);
        yield break;
    }

    public void SetProgressPercent(long fileSize, long processSize)
    {
        throw new System.NotImplementedException();
    }

    public string url;
    public byte[] postData;
    public Dictionary<string, string> headers;
    public WWW _www;
    public bool successflag;
    public float progress;
    private onCompleted onCompletedProc;
    public delegate void completedProc();
    public delegate void onCompleted(Request obj, bool isSuccess);
}
