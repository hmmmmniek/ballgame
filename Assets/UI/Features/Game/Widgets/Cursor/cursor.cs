using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class CursorController : Widget {
    public new const string ELEMENT_NAME = "Cursor";
    public new const string TEMPLATE_SELECTOR = "cursor.uxml";
    private VisualElement element;



    private int rollInputArrowTailMaxWidth = 60;
    private int spinInputArrowTailMaxWidth = 60;


    VisualElement defaultCursorContainer;
    VisualElement carryingCursorContainer;
    VisualElement attractingCursorContainer;
    VisualElement rollInputCursorContainer;
    VisualElement rollInputArrow;

    VisualElement rollInputArrowTail;
    VisualElement rollInputArrowHead;
    VisualElement rollInputArrowHeadShadow;
    VisualElement spinInputCursorContainer;
    VisualElement spinInputArrow;
    VisualElement spinInputArrowTail;
    VisualElement spinInputArrowHead;
    VisualElement spinInputArrowHeadShadow;
    VisualElement shieldCursorContainer;

    public int _specialCursorActive;
    public bool specialCursorActive {
        get {
            return _specialCursorActive > 0;
        }
        set {
            if(value) {
                _specialCursorActive = _specialCursorActive + 1;
            } else {
                _specialCursorActive = _specialCursorActive - 1;
            }
            if(_specialCursorActive < 0) {
                _specialCursorActive = 0;
            }
            if(specialCursorActive && !defaultCursorContainer.ClassListContains("inactive")) {
                defaultCursorContainer.AddToClassList("inactive");
            }
            if(!specialCursorActive && defaultCursorContainer.ClassListContains("inactive")) {
                defaultCursorContainer.RemoveFromClassList("inactive");
            }
        }
    }
    public CursorController(VisualElement element) {
        this.element = element;

        defaultCursorContainer = element.Q<VisualElement>("game__default-cursor__container");
        carryingCursorContainer = element.Q<VisualElement>("game__carrying-cursor__container");
        attractingCursorContainer = element.Q<VisualElement>("game__attracting-cursor__container");
        rollInputCursorContainer = element.Q<VisualElement>("game__roll-input-cursor__container");
        rollInputArrow = element.Q<VisualElement>("game__roll-input-cursor__section__lines__arrow"); 
        rollInputArrowTail = element.Q<VisualElement>("game__roll-input-cursor__section__lines__arrow__tail"); 
        rollInputArrowHeadShadow = element.Q<VisualElement>("game__roll-input-cursor__section__lines__arrow__head__shadow");
        rollInputArrowHead = element.Q<VisualElement>("game__roll-input-cursor__section__lines__arrow__head");
        spinInputCursorContainer = element.Q<VisualElement>("game__spin-input-cursor__container");
        spinInputArrow = element.Q<VisualElement>("game__spin-input-cursor__arrow"); 
        spinInputArrowTail = element.Q<VisualElement>("game__spin-input-cursor__arrow__tail"); 
        spinInputArrowHeadShadow = element.Q<VisualElement>("game__spin-input-cursor__arrow__head__shadow");
        spinInputArrowHead = element.Q<VisualElement>("game__spin-input-cursor__arrow__head");
        shieldCursorContainer = element.Q<VisualElement>("game__shield-cursor__container");

        Watch(GameState.Select<bool>(GameState.GetCarryingBall, (carrying) => {
            if(carrying) {
                specialCursorActive = true;
                carryingCursorContainer.RemoveFromClassList("inactive");
            } else {
                specialCursorActive = false;
                carryingCursorContainer.AddToClassList("inactive");
            }
        }));


        Watch(GameState.Select<bool>(GameState.GetAttracting, (attracting) => {
            if(attracting) {
                specialCursorActive = true;
                attractingCursorContainer.RemoveFromClassList("inactive");
            } else {
                specialCursorActive = false;
                attractingCursorContainer.AddToClassList("inactive");
            }
        }));

        Watch(GameState.Select<bool>(GameState.GetInputtingRoll, (inputtingRoll) => {
            if(inputtingRoll) {
                specialCursorActive = true;
                rollInputCursorContainer.RemoveFromClassList("inactive");
                carryingCursorContainer.AddToClassList("hide-dot");
                attractingCursorContainer.AddToClassList("hide-horizontal");
            } else {
                specialCursorActive = false;
                rollInputCursorContainer.AddToClassList("inactive");
                carryingCursorContainer.RemoveFromClassList("hide-dot");
                attractingCursorContainer.RemoveFromClassList("hide-horizontal");
            }
        }));

        Watch(GameState.Select<bool>(GameState.GetInputtingSpin, (inputtingSpin) => {
            if(inputtingSpin) {
                specialCursorActive = true;
                spinInputCursorContainer.RemoveFromClassList("inactive");
            } else {
                specialCursorActive = false;
                spinInputCursorContainer.AddToClassList("inactive");
            }
        }));

        Watch(GameState.Select<bool>(GameState.GetShielding, (shielding) => {
            if(shielding) {
                specialCursorActive = true;
                shieldCursorContainer.RemoveFromClassList("inactive");
            } else {
                specialCursorActive = false;
                shieldCursorContainer.AddToClassList("inactive");
            }
        }));

        Watch(GameState.Select<float>(GameState.GetRollInput, (rollInput) => {
            bool reverse = false;
            if(rollInput < 0) {
                rollInput = -rollInput;
                reverse = true;
            }
            if(reverse && !rollInputArrow.ClassListContains("reversed")) {
                rollInputArrow.AddToClassList("reversed");
            }
            if(!reverse && rollInputArrow.ClassListContains("reversed")) {
                rollInputArrow.RemoveFromClassList("reversed");
            }
            
            StyleLength amount = new StyleLength(new Length(rollInput * rollInputArrowTailMaxWidth, LengthUnit.Pixel));
            rollInputArrowTail.style.width = amount;
            rollInputArrowHead.style.right = amount;
            rollInputArrowHeadShadow.style.right = new StyleLength(new Length(rollInput * rollInputArrowTailMaxWidth - 1, LengthUnit.Pixel));
        }));


        Watch(GameState.Select<Vector2>(GameState.GetSpinInput, (spinInput) => {
          
            StyleLength amount = new StyleLength(new Length(spinInput.magnitude * spinInputArrowTailMaxWidth, LengthUnit.Pixel));
            spinInputArrowTail.style.width = amount;
            spinInputArrowHead.style.left = amount;
            spinInputArrowHeadShadow.style.left = new StyleLength(new Length(spinInput.magnitude * spinInputArrowTailMaxWidth - 1, LengthUnit.Pixel));
            float degrees = (float)(Math.Atan2(spinInput.y, spinInput.x)*180/Math.PI);
            spinInputArrow.style.rotate = new StyleRotate(new Rotate(new Angle(degrees, AngleUnit.Degree)));
            

        }));


    }

}
