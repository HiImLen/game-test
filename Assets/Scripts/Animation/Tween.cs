using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace LeanTweenAnimation.UI
{
    public class Tween : MonoBehaviour
    {
        public GameObject objectToAnimate;
        public UIAnimationTypes animationType;
        public LeanTweenType easeType;
        public float duration = 1f;
        public float delay = 0f;

        public bool loop;
        public bool pingpong;

        // If startPositionOffset is true, animation will start from the from position specified in the script.
        // If startPositionOffset is false, animation will start from the object's current position.
        public bool startPositionOffset = true;
        public Vector3 from;
        public Vector3 to;
        public bool autoDisable = false;
        public float autoDisableDelay = 0f;

        public bool showOnEnable = true;
        public bool ignoreTimeScale = true;
        public UnityEvent onCompleteCallback;

        private LTDescr tweenObject;
        private LTDescr showTween;
        private LTDescr disableTween;
        public Action OnDisableWithEaseTypeComplete;
        void OnEnable()
        {
            if (showOnEnable)
                Show();
        }

        public void Show()
        {
            HandleTween(true);
            showTween = tweenObject;
        }

        public void Show(Action onCompleteCallback)
        {
            gameObject.SetActive(true);
            HandleTween(true);
            showTween = tweenObject;

            tweenObject.setOnComplete(() =>
            {
                showTween = null;
                onCompleteCallback?.Invoke();
            });
        }

        public void ShowReverse()
        {
            SwapDirection();
            HandleTween(true);
            showTween = tweenObject;
            tweenObject.setOnComplete(() =>
            {
                showTween = null;
            });
            SwapDirection();
        }

        void HandleTween(bool show)
        {
            if (show && showTween != null)
                return;
            if (show && disableTween != null)
            {
                LeanTween.cancel(disableTween.id);
                disableTween = null;
            }
            if (!show && showTween != null)
            {
                LeanTween.cancel(showTween.id);
                showTween = null;
            }
            if (!show && disableTween != null)
                return;
            if (objectToAnimate == null)
                objectToAnimate = gameObject;

            switch (animationType)
            {
                case UIAnimationTypes.Move:
                    MoveAbsolute();
                    break;
                case UIAnimationTypes.MoveX:
                    MoveAbsolute();
                    break;
                case UIAnimationTypes.MoveY:
                    MoveAbsolute();
                    break;
                case UIAnimationTypes.MoveZ:
                    MoveAbsolute();
                    break;
                case UIAnimationTypes.Scale:
                    Scale();
                    break;
                case UIAnimationTypes.ScaleX:
                    Scale();
                    break;
                case UIAnimationTypes.ScaleY:
                    Scale();
                    break;
                case UIAnimationTypes.ScaleZ:
                    Scale();
                    break;
                case UIAnimationTypes.Spin:
                    Spin();
                    break;
                case UIAnimationTypes.Fade:
                    Fade();
                    break;
            }

            tweenObject.setDelay(delay);
            tweenObject.setEase(easeType);
            tweenObject.setIgnoreTimeScale(ignoreTimeScale);
            tweenObject.setOnComplete(() =>
            {
                if (show) showTween = null;
                else disableTween = null;
                if (autoDisable) Disable(autoDisableDelay);
                onCompleteCallback.Invoke();
            });

            if (loop)
            {
                tweenObject.loopCount = int.MaxValue;
                tweenObject.setLoopClamp();
            }
            else if (pingpong)
                tweenObject.setLoopPingPong();
        }

        public void Fade()
        {
            if (gameObject.GetComponent<CanvasGroup>() == null)
                gameObject.AddComponent<CanvasGroup>();
            if (startPositionOffset)
                objectToAnimate.GetComponent<CanvasGroup>().alpha = from.x;
            tweenObject = LeanTween.alphaCanvas(objectToAnimate.GetComponent<CanvasGroup>(), to.x, duration);
        }

        public void MoveAbsolute()
        {
            if (startPositionOffset)
            {
                if (animationType == UIAnimationTypes.Move)
                    transform.localPosition = from;
                else if (animationType == UIAnimationTypes.MoveX)
                    transform.localPosition = new Vector3(from.x, transform.localPosition.y, transform.localPosition.z);
                else if (animationType == UIAnimationTypes.MoveY)
                    transform.localPosition = new Vector3(transform.localPosition.x, from.y, transform.localPosition.z);
                else if (animationType == UIAnimationTypes.MoveZ)
                    transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, from.z);
            }

            if (animationType == UIAnimationTypes.Move)
                tweenObject = LeanTween.moveLocal(objectToAnimate, to, duration);
            else if (animationType == UIAnimationTypes.MoveX)
                tweenObject = LeanTween.moveLocalX(objectToAnimate, to.x, duration);
            else if (animationType == UIAnimationTypes.MoveY)
                tweenObject = LeanTween.moveLocalY(objectToAnimate, to.y, duration);
            else if (animationType == UIAnimationTypes.MoveZ)
                tweenObject = LeanTween.moveLocalZ(objectToAnimate, to.z, duration);
        }

        public void Scale()
        {
            if (startPositionOffset)
            {
                if (animationType == UIAnimationTypes.Scale)
                    transform.localScale = from;
                else if (animationType == UIAnimationTypes.ScaleX)
                    transform.localScale = new Vector3(from.x, transform.localScale.y, transform.localScale.z);
                else if (animationType == UIAnimationTypes.ScaleY)
                    transform.localScale = new Vector3(transform.localScale.x, from.y, transform.localScale.z);
                else if (animationType == UIAnimationTypes.ScaleZ)
                    transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, from.z);
            }

            if (animationType == UIAnimationTypes.Scale)
                tweenObject = LeanTween.scale(objectToAnimate, to, duration);
            else if (animationType == UIAnimationTypes.ScaleX)
                tweenObject = LeanTween.scaleX(objectToAnimate, to.x, duration);
            else if (animationType == UIAnimationTypes.ScaleY)
                tweenObject = LeanTween.scaleY(objectToAnimate, to.y, duration);
            else if (animationType == UIAnimationTypes.ScaleZ)
                tweenObject = LeanTween.scaleZ(objectToAnimate, to.z, duration);
        }

        public void Spin()
        {
            if (startPositionOffset)
                transform.rotation = Quaternion.Euler(from);
            tweenObject = LeanTween.rotateAround(objectToAnimate, Vector3.up, to.y, duration);
        }

        void SwapDirection()
        {
            if (animationType == UIAnimationTypes.Spin)
            {
                to = new Vector3(to.x, to.y, -to.z);
                return;
            }
            (from, to) = (to, from);
        }

        public void Disable()
        {
            SwapDirection();
            HandleTween(false);
            SwapDirection();
            disableTween = tweenObject;
            tweenObject.setOnComplete(() =>
            {
                gameObject.SetActive(false);
                disableTween = null;
            });
        }

        public void Disable(float delay)
        {
            SwapDirection();
            float temp = this.delay;
            this.delay = delay;
            HandleTween(false);
            SwapDirection();
            disableTween = tweenObject;
            tweenObject.setOnComplete(() =>
            {
                gameObject.SetActive(false);
                disableTween = null;
            });
            this.delay = temp;
        }

        public void DisableWithEaseType()
        {
            LeanTweenType temp = this.easeType;
            // Set ease type to the previous one
            this.easeType = (LeanTweenType)((int)this.easeType - 1);
            SwapDirection();
            HandleTween(false);
            SwapDirection();
            disableTween = tweenObject;
            this.easeType = temp;
            tweenObject.setOnComplete(() =>
            {
                gameObject.SetActive(false);
                disableTween = null;
                OnDisableWithEaseTypeComplete?.Invoke();
            });
        }

        public void Disable(Action onCompleteAction)
        {
            SwapDirection();
            HandleTween(false);
            SwapDirection();
            disableTween = tweenObject;
            tweenObject.setOnComplete(() =>
            {
                gameObject.SetActive(false);
                disableTween = null;
                onCompleteAction?.Invoke();
            });
        }

        public void DisableWithEaseType(Action onCompleteAction)
        {
            LeanTweenType temp = this.easeType;
            // Set ease type to the previous one
            this.easeType = (LeanTweenType)((int)this.easeType - 1);
            SwapDirection();
            HandleTween(false);
            SwapDirection();
            disableTween = tweenObject;
            this.easeType = temp;
            tweenObject.setOnComplete(() =>
            {
                gameObject.SetActive(false);
                disableTween = null;
                onCompleteAction?.Invoke();
            });
        }


        public void ActivateGO(GameObject obj)
        {
            ActivateRecursively(obj);
            obj.SetActive(true);
        }

        private void ActivateRecursively(GameObject obj)
        {
            if (obj.GetComponent<UITween>() != null)
                obj.gameObject.SetActive(true);
            foreach (Transform child in obj.transform)
                ActivateRecursively(child.gameObject);
        }

        public void DisableGO(GameObject obj)
        {
            if (obj.GetComponent<UITween>() != null)
            {
                UITween[] tweens = obj.GetComponents<UITween>();
                foreach (UITween tween in tweens)
                    tween.Disable();
            }
            foreach (Transform child in obj.transform)
                DisableGO(child.gameObject);
        }
    }
}