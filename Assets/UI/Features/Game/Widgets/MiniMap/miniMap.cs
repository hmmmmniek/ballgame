using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using static CreateSessionController;

public class MiniMapController : Widget {
    public new const string ELEMENT_NAME = "MiniMap";
    public new const string TEMPLATE_SELECTOR = "miniMap.uxml";
    private VisualElement element;

    float mapWidth = 0;
    float mapLength = 0;
    float mapHeight = 0;
    float mapGoalWidth = 0;
    float mapGoalHeight = 0;
    float mapGoalDepth = 0;
    float mapGoalPostRadius = 0;

    float pixelsPerMeter;
    float centerY;
    float centerX;



    private float miniMapWidth = 200;
    private float miniMapHeight = 0;
    private (BallGunController ballGunController, PlayerController playerController, VisualElement element)[] players;
    private BallController ball;
    VisualElement container;
    VisualElement field;
    VisualElement ballElement;
    VisualElement goalSouth;
    VisualElement goalNorth;
    public MiniMapController(VisualElement element) {
        this.element = element;
        container = element.Q<VisualElement>("miniMap__container");
        field = element.Q<VisualElement>("miniMap__field");
        ballElement = element.Q<VisualElement>("miniMap__field__ball");
        goalSouth = element.Q<VisualElement>("miniMap__field__goal-south");
        goalNorth = element.Q<VisualElement>("miniMap__field__goal-north");
        

        MapGenerator gen = new MapGenerator();
        NetworkState.Select<MapSize?>(NetworkState.GetMapSize, (mapSize) => {
            if(!mapSize.HasValue) {
                return;
            }

            switch (mapSize.Value) {
                case MapSize.Small: {
                    mapWidth = gen.mapSmallWidth;
                    mapLength = gen.mapSmallLength;
                    mapHeight = gen.mapSmallHeight;
                    mapGoalWidth = gen.mapSmallGoalWidth;
                    mapGoalHeight = gen.mapSmallGoalHeight;
                    mapGoalDepth = gen.mapSmallGoalDepth;
                    mapGoalPostRadius = gen.mapSmallGoalPostRadius;
                    break;
                }
                case MapSize.Medium: {
                    mapWidth = gen.mapMediumWidth;
                    mapLength = gen.mapMediumLength;
                    mapHeight = gen.mapMediumHeight;
                    mapGoalWidth = gen.mapMediumGoalWidth;
                    mapGoalHeight = gen.mapMediumGoalHeight;
                    mapGoalDepth = gen.mapMediumGoalDepth;
                    mapGoalPostRadius = gen.mapMediumGoalPostRadius;
                    break;
                }
                case MapSize.Large: {
                    mapWidth = gen.mapLargeWidth;
                    mapLength = gen.mapLargeLength;
                    mapHeight = gen.mapLargeHeight;
                    mapGoalWidth = gen.mapLargeGoalWidth;
                    mapGoalHeight = gen.mapLargeGoalHeight;
                    mapGoalDepth = gen.mapLargeGoalDepth;
                    mapGoalPostRadius = gen.mapLargeGoalPostRadius;
                    break;
                }
            }

            miniMapHeight = miniMapWidth / (mapWidth / mapLength);

            pixelsPerMeter = miniMapHeight / mapLength;
            centerY = miniMapHeight / 2;
            centerX = miniMapWidth / 2;

            SetMiniMap();
        });

        GameState.Select<(BallGunController ballGunController, PlayerController playerController)[]>(GameState.GetPlayers, (players) => {
            SetPlayers(players);
        });

        GameState.Select<BallController>(GameState.GetBall, (ball) => {
            this.ball = ball;
        });

        UpdateMiniMap();
    }

    private void SetMiniMap() {
        StyleLength amount = new StyleLength(new Length(miniMapHeight, LengthUnit.Pixel));
        field.style.height = amount;

        float diagonal = (float)Math.Sqrt(Math.Pow(miniMapHeight, 2) + Math.Pow(miniMapWidth, 2));
        container.style.right = new StyleLength(new Length((diagonal / 2) - (miniMapWidth / 2), LengthUnit.Pixel));
        container.style.bottom = new StyleLength(new Length((diagonal / 2) - (miniMapHeight / 2), LengthUnit.Pixel));

        float goalLengthPx = mapGoalDepth * pixelsPerMeter;
        float goalWidthPx = mapGoalWidth * pixelsPerMeter;
        goalSouth.style.bottom = new StyleLength(new Length(-goalLengthPx, LengthUnit.Pixel));
        goalSouth.style.left = new StyleLength(new Length(centerX - goalWidthPx/2, LengthUnit.Pixel));
        goalSouth.style.height = new StyleLength(new Length(goalLengthPx, LengthUnit.Pixel));
        goalSouth.style.width = new StyleLength(new Length(goalWidthPx, LengthUnit.Pixel));

        goalNorth.style.top = new StyleLength(new Length(-goalLengthPx, LengthUnit.Pixel));
        goalNorth.style.left = new StyleLength(new Length(centerX - goalWidthPx/2, LengthUnit.Pixel));
        goalNorth.style.height = new StyleLength(new Length(goalLengthPx, LengthUnit.Pixel));
        goalNorth.style.width = new StyleLength(new Length(goalWidthPx, LengthUnit.Pixel));

  
    }

    private void SetPlayers((BallGunController ballGunController, PlayerController playerController)[] players) {
        (BallGunController ballGunController, PlayerController playerController, VisualElement element)[] output = new (BallGunController ballGunController, PlayerController playerController, VisualElement element)[players.Length];
        int i = 0;
        if(this.players != null) {
            foreach (var player in this.players) {
                if(player.element != null) {
                    field.Remove(player.element);
                }
            }
        }

        foreach (var player in players) {
            VisualElement playerElement = new VisualElement();
            playerElement.AddToClassList("miniMap__field__player");
            if(player.playerController != null && PlayerController.Local != null) {
                if(player.playerController.Id == PlayerController.Local.Id) {
                    playerElement.AddToClassList("local");
                }
                field.Insert(field.childCount - 2, playerElement);
                output[i] = (ballGunController: player.ballGunController, playerController: player.playerController, element: playerElement);
                i = i + 1;
            }

        }
        this.players = output;
    }
    private async void UpdateMiniMap() {
        await Task.Delay(10);
        if(players != null) {
            foreach (var player in players) {
                if(player.playerController != null && PlayerController.Local != null) {

                    if(PlayerController.Local.Id == player.playerController.Id) {
                        container.style.rotate = new StyleRotate(new Rotate(new Angle(-player.playerController.transform.rotation.eulerAngles.y, AngleUnit.Degree)));
                    }

                    float playerOffsetFromCenterY = player.playerController.transform.position.z * pixelsPerMeter;
                    float playerPosY = centerY + playerOffsetFromCenterY;

                    float playerOffsetFromCenterX = player.playerController.transform.position.x * pixelsPerMeter;
                    float playerPosX = centerX + playerOffsetFromCenterX;

                    player.element.style.bottom = new StyleLength(new Length(playerPosY, LengthUnit.Pixel));
                    player.element.style.left = new StyleLength(new Length(playerPosX, LengthUnit.Pixel));
                    player.element.style.rotate = new StyleRotate(new Rotate(new Angle(player.playerController.transform.rotation.eulerAngles.y + 45, AngleUnit.Degree)));

                }
            }
        }
        if(ball != null) {
            float offsetFromCenterY = ball.transform.position.z * pixelsPerMeter;
            float ballPosY = centerY + offsetFromCenterY;

            float offsetFromCenterX = ball.transform.position.x * pixelsPerMeter;
            float ballPosX = centerX + offsetFromCenterX;

            ballElement.style.bottom = new StyleLength(new Length(ballPosY, LengthUnit.Pixel));
            ballElement.style.left = new StyleLength(new Length(ballPosX, LengthUnit.Pixel));
        }

        UpdateMiniMap();
    }
}

