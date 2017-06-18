using UnityEngine;
using System.Collections;

// 挂在粒子上，然后指定一个Widget，然后它会抓取widget所在panel的dc，
// 然后设置自身的mat.renderqueue = dc.renderqueue
public class UIParticle : MonoBehaviour {
    [Tooltip("目标Widget，Particle会拾取它的RenderQueue")]
    public UIWidget widget;
    [Tooltip("相对目标Widget RenderQueue的偏移")]
    public int offset = 1;
    [Tooltip("只在start阶段更新一次")]
    public bool updateOnce = true;
    [Tooltip("固定Queue，只有大于0才生效")]
    public int fixQueue = -1;

    Renderer cacheRenderer;

    private bool changed = false;
    int lastRendererQueue = -1;

    void Start()
    {
        if (widget == null) {
            FindWidget(transform);
        }
    }

    bool FindWidget(Transform obj)
    {
        if (obj == null || obj.parent == null)
            return false;

        var w = obj.parent.GetComponent<UIWidget>();
        if (w == null){
            return FindWidget(obj.parent);
        } else {
            widget = w;
            return true;
        }
    }


    public bool AdjustRenderQueue()
    {
        if ((widget == null || widget.drawCall == null) &&
            fixQueue < 0)
            return false;

        if (cacheRenderer == null)
        {
            cacheRenderer = GetComponent<Renderer>();
        }


        if (cacheRenderer != null && cacheRenderer.material != null)
		{
            int curRQ = 0;
            if (fixQueue > 0)
            {
                curRQ = fixQueue;
            }
            else
            {
                curRQ = widget.drawCall.renderQueue;
            }

            if (lastRendererQueue != curRQ)
            {
                lastRendererQueue = curRQ;
                cacheRenderer.material.renderQueue = curRQ + offset;
                return true;
            }
		}
        return false;
    }

    void LateUpdate()
    {
        if (changed && updateOnce)
            return;

        if (AdjustRenderQueue())
        {
            if (!changed)
            {
                changed = true;
				gameObject.SendMessage("UIParticleRendererChanged", SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}
