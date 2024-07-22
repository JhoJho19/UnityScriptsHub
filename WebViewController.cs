using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    UniWebView webView;
    [SerializeField] RectTransform policyRectTransform;
    [SerializeField] GameObject policyPanel;
    [SerializeField] string policyURL;
    [SerializeField] RectTransform termsRectTransform;
    [SerializeField] GameObject termsPanel;
    [SerializeField] string termsURL;
    [SerializeField] RectTransform supRectTransform;
    [SerializeField] GameObject supPanel;
    [SerializeField] string infoURL;

    public void ShowPP()
    {
        policyPanel.SetActive(true);

        var webViewGameObject = new GameObject("UniWebView");
        webView = webViewGameObject.AddComponent<UniWebView>();

        webView.ReferenceRectTransform = policyRectTransform;
        webView.Show(true, UniWebViewTransitionEdge.Bottom, 0.35f);
        webView.Hide(true, UniWebViewTransitionEdge.Bottom, 0.35f);
        webView.Load(policyURL);
        webView.Show();

        webView.OnShouldClose += (view) => {
            webView = null;
            return true;
        };

        webView.OnMessageReceived += (view, message) => {
            if (message.Path.Equals("close"))
            {
                Destroy(webView);
                webView = null;
            }
        };
    }

    public void ShowTerms()
    {
        termsPanel.SetActive(true);

        var webViewGameObject = new GameObject("UniWebView");
        webView = webViewGameObject.AddComponent<UniWebView>();

        webView.ReferenceRectTransform = termsRectTransform;
        webView.Show(true, UniWebViewTransitionEdge.Bottom, 0.35f);
        webView.Hide(true, UniWebViewTransitionEdge.Bottom, 0.35f);
        webView.Load(termsURL);
        webView.Show();

        webView.OnShouldClose += (view) => {
            webView = null;
            return true;
        };

        webView.OnMessageReceived += (view, message) => {
            if (message.Path.Equals("close"))
            {
                Destroy(webView);
                webView = null;
            }
        };
    }

    public void ShowSup()
    {
        supPanel.SetActive(true);

        var webViewGameObject = new GameObject("UniWebView");
        webView = webViewGameObject.AddComponent<UniWebView>();

        webView.ReferenceRectTransform = supRectTransform;
        webView.Show(true, UniWebViewTransitionEdge.Bottom, 0.35f);
        webView.Hide(true, UniWebViewTransitionEdge.Bottom, 0.35f);
        webView.Load(infoURL);
        webView.Show();

        webView.OnShouldClose += (view) => {
            webView = null;
            return true;
        };

        webView.OnMessageReceived += (view, message) => {
            if (message.Path.Equals("close"))
            {
                Destroy(webView);
                webView = null;
            }
        };
    }

    public void ClosePP()
    {
        policyPanel.SetActive(false);
        Destroy(webView);
        webView = null;
    }

    public void CloseTerms()
    {
        termsPanel.SetActive(false);
        Destroy(webView);
        webView = null;
    }

    public void CloseSup()
    {
        supPanel.SetActive(false);
        Destroy(webView);
        webView = null;
    }

}