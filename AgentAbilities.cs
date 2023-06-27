using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;

using StarterAssets;

public class AgentAbilities : Agent
{
    public bool doAgentRack = false;
    public bool doAgentNewGame = false;
    public bool bowl = false;

    float episodeReward = 0f;

    int _trainingScoreLevel1 = 30;
    int _trainingScoreLevel2 = 60;
    int _trainingScoreLevel3 = 90;
    int _highScore = 0;

    /***
   float pinsHitReward = 0.1f; // Increased reward for hitting pins
    float highScoreReward = 1f; // Increased reward for achieving a new high score
    float gutterPenalty = -1.0f;
    float gutterReward = 0.5f;
    float _trainingScoreLevel1Penalty = -1f;
    float _trainingScoreLevel1Reward = 0.1f;
    float _trainingScoreLevel2Penalty = 0f;
    float _trainingScoreLevel2Reward = 0.2f;
    float _trainingScoreLevel3Penalty = 0f;
    float _trainingScoreLevel3Reward = 0.3f;
    float strikeReward = 0.5f; // Increased reward for achieving a strike
    float spareReward = 0.4f; // Increased reward for achieving a spare
    float ballSidePenalty = 0f; // Negative reward for rolling the ball too far to the side
    float finalGameScoreRewardLevel1 = 5f; // Increased reward for achieving the target game score
    float finalGameScoreRewardLevel2 = 10f; // Increased reward for achieving the target game score
    float finalGameScoreRewardLevel3 = 15f; // Increased reward for achieving the target game score
    float finalGameScorePenalty = -15f; // Reduced penalty for falling below the target game score
    float clampedPenalty = 0f;
    ***/


    float pinsHitReward = 0.25f; // Increased reward for hitting pins
    float highScoreReward = 0.5f; // Increased reward for achieving a new high score
    //float gutterPenalty = -2.0f;
    float gutterPenalty = -0.5f;
    float gutterReward = 0.05f;
    float strikeReward = 2f; // Increased reward for achieving a strike
    float spareReward = 1f; // Increased reward for achieving a spare

    float _trainingScoreLevel1Reward = 0.025f;
    float _trainingScoreLevel2Reward = 0.05f;
    float _trainingScoreLevel3Reward = 0.1f;

    float _trainingScoreLevel1Penalty = 0f;
    float _trainingScoreLevel2Penalty = 0f;
    float _trainingScoreLevel3Penalty = 0f;
    
 
    float ballSidePenalty = 0f; // Negative reward for rolling the ball too far to the side
    float finalGameScoreRewardLevel1 = 1f; // Increased reward for achieving the target game score
    float finalGameScoreRewardLevel2 = 2f; // Increased reward for achieving the target game score
    float finalGameScoreRewardLevel3 = 3f; // Increased reward for achieving the target game score
    float finalGameScorePenalty = 0f; // Reduced penalty for falling below the target game score
    float clampedPenalty = 0f;





    private float speed = 1f;
    private float moveX = 0f;
    private float moveY = 0f;
    private float aimX = 0f;
    private float aimY = 0f;
    private float aimZ = 0f;

    private float startTime;
    private float waitTime = 30f; // The time to wait in seconds

    private Queue<AgentAction> lastActions = new Queue<AgentAction>();
    private int actionHistoryLength = 21;
    bool heuristicOnly = false;
    
    [SerializeField]
    private bool hasMoved = false;
    [SerializeField]
    private bool ballMovedToPosition = false;
    
    PlayerAbilities _playerAbilities;
    ThirdPersonController _thirdPersonController;
    Vector2 agentMovement = Vector2.zero;
    AimAssistHandler _aimAssistHandler;
    Vector3 _startPosition;
    Frame currentFrameData;

    Lane lane
    {
        get
        {
            return _playerAbilities.lane;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _playerAbilities = GetComponent<PlayerAbilities>();
        _thirdPersonController = GetComponent<ThirdPersonController>();
        _aimAssistHandler = GetComponent<AimInputHandler>().aimAssistHandler;

        _startPosition = transform.position;

        bowl = true;

    }

    // Update is called once per frame
    void Update()
    {
        //if (!lane.isBowling && lane.allowBowling && !_playerAbilities.bowlingApproachStarted)
        if (bowl)
        {
            bowl = false;

            _playerAbilities.PickUpBall();

            //Debug.Log($"{gameObject.name} : {lane.isBowling} : {lane.allowBowling} : {_playerAbilities.bowlingApproachStarted}");

            //lane.allowBowling = false;
            //lane.isBowling = true;

            CheckBowlingFlag();
        }
        else
        {
            //Debug.Log($"{gameObject.name} : {lane.isBowling} : {lane.allowBowling} : {_playerAbilities.bowlingApproachStarted}");

        }



        CheckAgentWaitTime();
        CheckTrainingOptions();

    }

    private void FixedUpdate()
    {
        
    }


    void CheckAgentWaitTime()
    {
        if (startTime != 0f)
        {
            // Calculate the time difference
            float elapsedTime = Time.unscaledTime - startTime;
            // Check if the wait time has passed
            if (elapsedTime >= waitTime)
            {
                // Reset the start time for the next wait period
                startTime = 0f;

                // Perform the desired action
                //Debug.Log($"{gameObject.name} wait time has passed. Action performed.", gameObject);

                // Reset the agent here
                if (_playerAbilities.bowlingBall != null)
                {
                    _playerAbilities.ReleaseBall();
                    doAgentRack = true;
                }               
            }
        }
       

    }

    void CheckBowlingFlag()
    {
        //Debug.Log($"{gameObject.name} CheckBowlingFlag {lane.isBowling}");

        startTime = Time.unscaledTime;
        MoveAgentToPosition();
    }


  



    void CheckTrainingOptions()
    {
        if (doAgentRack)
        {
            doAgentRack = false;
            Debug.Log($"doAgentRack {gameObject.name}");
            lane.currentBowler.currentRoll--;

            lane.Rerack(false);
            _playerAbilities._ballInHand = false;
            /***
            if (_playerAbilities.bowlingBall != null)
            {
                Destroy(_playerAbilities.bowlingBall);
            }
            ***/

        }
        if (doAgentNewGame)
        {
            doAgentNewGame = false;

            lane.scoring.ResetGame();
        }

        if (_playerAbilities._ballInHand && _playerAbilities.bowlingBall == null)
        {
            _playerAbilities._ballInHand = false;
        }
    }

    
    //Handles only gutter ball training
    public void UpdateEpisodeReward(int currentFrame, int currentRoll, Frame currentFrameData, Roll currentRollData)
    {
       
        if (currentRollData.inGutter)
        {
            episodeReward += gutterPenalty;
        }
        else
        {
            episodeReward += gutterReward;
            episodeReward += currentRollData.pins.Count * pinsHitReward;
        }

       /***
        if (currentFrameData.IsStrike())
            episodeReward += strikeReward;
        else if (currentFrameData.IsSpare())
            episodeReward += spareReward;
       ***/
     
        //Debug.Log($"IsStrike {currentFrameData.IsStrike()} : IsSpare {currentFrameData.IsSpare()}");

        SetReward(episodeReward);
      
        //Debug.Log($"currentFrame: {currentFrame} currentRoll: {currentRoll}");

       
          if (currentFrame == 10)
          {
              switch (currentRoll)
              {
                  case 2:
                      if (currentFrameData.IsSpare() || currentFrameData.IsStrike())
                      {
                          break;
                      }
                      else
                      {
                          HandleEndEpisode();
                      }
                      break;
                  case 3:
                      HandleEndEpisode();
                      break;
              }
          }
          else if (currentRoll == 2)
          {
              HandleEndEpisode();
          }
         

    }
   
    void HandleEndEpisode()
    {
        Debug.Log(gameObject.name + " gutter check reward " + episodeReward);

        EndEpisode();
    }
    
    //Handles all frames
    public void UpdateEpisodeReward(Frame[] frames, int currentFrame, int currentRoll)
    {
        try
        {
            int _currentFrameIndex = currentFrame - 1;
            int _currentRollIndex = currentRoll - 1;
            Frame currentFrameData = frames[_currentFrameIndex];
            Roll currentRollData = frames[_currentFrameIndex].rolls[_currentRollIndex];

            //Debug.Log($"currentFrame: {currentFrame} currentRoll: {currentRoll}");

            // Check if the frame was a strike or spare and adjust rewards accordingly
            if (currentFrameData.IsStrike())
            {
                episodeReward += strikeReward;
                //AddReward(strikeReward);
            }
            else if (currentFrameData.IsSpare())
            {
                episodeReward += spareReward;
                //AddReward(spareReward);
            }

            //Debug.Log($"Frame {_currentFrame} max score { ScoreManager.Singleton.CalculateMaxPossibleScore(frames)}");


            episodeReward += (lane.scoring.CalculateMaxPossibleScore(frames) < _trainingScoreLevel1) ? _trainingScoreLevel1Penalty : _trainingScoreLevel1Reward;
            episodeReward += (lane.scoring.CalculateMaxPossibleScore(frames) < _trainingScoreLevel2) ? _trainingScoreLevel2Penalty : _trainingScoreLevel2Reward;
            episodeReward += (lane.scoring.CalculateMaxPossibleScore(frames) < _trainingScoreLevel3) ? _trainingScoreLevel3Penalty : _trainingScoreLevel3Reward;


            // If the previous roll was in the gutter, apply a penalty
            if (currentRollData.inGutter)
            {
                episodeReward += gutterPenalty;
                //AddReward(gutterPenalty);

                // Calculate maxBallSideThreshold based on lane width or other relevant parameters
                float maxBallSideThreshold = lane.laneWidth * 0.5f; // Adjust as needed based on lane dimensions

                // Add the following check after the gutter penalties
                if (Mathf.Abs(currentRollData.aimPosition.x) > maxBallSideThreshold)
                {
                    //Debug.Log("Ball Position: " + previousRoll.aimPosition.x);
                    episodeReward += ballSidePenalty;
                }
            }
            else
            {
                episodeReward += gutterReward;
                //AddReward(gutterReward);
            }

            // If the roll hit any pins, apply a proportional reward
            episodeReward += currentRollData.pins.Count * pinsHitReward;
            //AddReward(previousRoll.pins.Count * pinsHitReward);

            // Check if the frame's score met the threshold and adjust rewards accordingly
            /***
            if (currentFrameData.frameScore >= frameScoreThresholdLevel1)
            {
                episodeReward += frameScoreRewardLevel1;
            }
            else
            {
                episodeReward += frameScorePenaltyLevel1;
            }
            ***/

            if (currentFrame == 10 && (currentRoll == 2 && (!currentFrameData.IsStrike() && !currentFrameData.IsSpare())) || currentRoll == 3)

            // If it's the end of the game, check if the final score met the training goal and adjust rewards accordingly
            //if (currentFrame == 10 && (currentRoll == 3 && !currentFrameData.IsStrike() && !currentFrameData.IsSpare()) || currentRoll == 4)
            {
                int finalScore = frames[9].cumulativeScore;

                if (finalScore >= _trainingScoreLevel1)
                {
                    if (finalScore >= _trainingScoreLevel3)
                    {
                        episodeReward += finalGameScoreRewardLevel3;
                    }
                    else if (finalScore >= _trainingScoreLevel2)
                    {
                        episodeReward += finalGameScoreRewardLevel2;
                    }
                    else if (finalScore >= _trainingScoreLevel1)
                    {
                        episodeReward += finalGameScoreRewardLevel1;
                    }
                }
                else
                {
                    episodeReward += finalGameScorePenalty;
                }



                if (finalScore > _highScore)
                {
                    _highScore = finalScore;
                    episodeReward += highScoreReward;
                    Debug.Log($"{_playerAbilities.gameObject.name} has a new high score: {_highScore}");

                }

                Debug.Log(gameObject.name + " final score " + finalScore + " : reward " + episodeReward);

                SetReward(episodeReward);

                EndEpisode();

                Debug.Log("End Episode");



            }
        }
        catch (IndexOutOfRangeException e)
        {
            Debug.Log($"currentFrame: {currentFrame} currentRoll: {currentRoll}");
        }



    }

    void CheckIsClamped()
    {
        if (_thirdPersonController.isClamped)
        {
            episodeReward += clampedPenalty;
         }

    }

    void MoveAgentToPosition()
    {
        //Debug.Log($"{gameObject.name} MoveAgentToPosition");

        Vector2 move = new Vector2(moveX, moveY);

        _thirdPersonController.agentMovement = move;

        StartCoroutine(CheckPlayerPositionCoroutine());

        StartCoroutine(MoveBallToPosition());


    }

    IEnumerator MoveBallToPosition()
    {
        //Debug.Log($"{gameObject.name}  MoveBallToPosition");

        //Debug.Log($"aimY: {aimY}");

        _aimAssistHandler.AgentAimAssist(new Vector3(0f, aimY, 0f));

        yield return new WaitUntil(() => !_aimAssistHandler.isMoving);

        //_playerAbilities.bowlingApproachStarted = false;

        lane.currentBowler.SetAimPosition(lane.laneSpotter.transform.localPosition);

        _playerAbilities.StartBowlingApproach();
    }


    private IEnumerator CheckPlayerPositionCoroutine()
    {
       while (_thirdPersonController.isMovingToDestination)
        {
            //Debug.Log("Stuck here");

            // Wait for 0.25 seconds before checking again
            yield return new WaitForSeconds(0.25f);
        }
    }

    public IEnumerator ResetAgentSettings()
    {
        yield return new WaitForSeconds(2.5f);

        ballMovedToPosition = false;
        hasMoved = false;
    }

    /***
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        heuristicOnly = true;
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = agentMovement.x;
        continuousActions[1] = agentMovement.y;

    }
    ***/

    public override void OnActionReceived(ActionBuffers actions)
    {
        try
        {
            moveX = speed * Mathf.Clamp(actions.ContinuousActions[0], -lane.minX, lane.maxX);
            //current only right handed, update for left handed
            moveY = speed * Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);

            //Clamped for right handed bowler
            aimY = actions.ContinuousActions[2];
          
            if (lastActions.Count >= actionHistoryLength)
            {
                lastActions.Dequeue();  // Remove the oldest action if we're at capacity
            }


            lastActions.Enqueue(new AgentAction { move = new Vector2(moveX, moveY), aim = new Vector3(0f, aimY, 0f)});  

            //for integers
            //Debug.Log(actions.DiscreteActions[0]);
        }
        catch (NullReferenceException) { }


    }

   
    public override void CollectObservations(VectorSensor sensor)
    {
        Bowler bowler = new Bowler();

        try
        {
            bowler = lane.bowlers.Find(x => x.playerObject == gameObject);
            Frame[] frames = bowler.frames;
            int currentFrameIndex = bowler.currentFrame - 1;
            int currentRollIndex = bowler.currentRoll - 1;

            if (bowler.currentRoll == 0)
                return;


            //Debug.Log($"currentFrame {bowler.currentFrame} : currentRoll {bowler.currentRoll}" );

            Vector3 releasePosition = frames[currentFrameIndex].rolls[currentRollIndex].releasePosition;

            sensor.AddObservation((int)lane.selectedPattern);
            sensor.AddObservation(lane.laneLength / 100); // Assuming lane length is large, normalizing to smaller range
            sensor.AddObservation(lane.laneWidth / 10); // Assuming lane width is large, normalizing to smaller range

            sensor.AddObservation(bowler.score / 300); // Normalizing assuming maximum score is 300
            sensor.AddObservation(bowler.currentFrame / 10); // Normalizing assuming maximum frames are 10
            sensor.AddObservation(bowler.currentRoll / 3); // Normalizing assuming maximum rolls are 3
            sensor.AddObservation(_highScore / 300); // Normalizing assuming maximum high score is 300

            // Provide relative position observations
            sensor.AddObservation(releasePosition.normalized);
            //sensor.AddObservation(lane.surface.transform.localPosition - releasePosition);
            //sensor.AddObservation(lane.leftGutter.transform.localPosition - releasePosition);
            //sensor.AddObservation(lane.rightGutter.transform.localPosition - releasePosition);

            sensor.AddObservation(frames[currentFrameIndex].rolls[currentRollIndex].pins.Count/10); // Normalizing assuming maximum pins are 10
            sensor.AddObservation(frames[currentFrameIndex].rolls[currentRollIndex].inGutter ? 1 : 0);
            sensor.AddObservation(frames[currentFrameIndex].rolls[currentRollIndex].aimPosition.normalized); // Normalizing based on lane width

           
            // Scoring
            sensor.AddObservation(frames[currentFrameIndex].cumulativeScore / 300); // Normalizing assuming maximum cumulative score is 300
            sensor.AddObservation(frames[currentFrameIndex].frameScore / 30); // Normalizing assuming maximum frame score is 30

            foreach (GameObject p in lane.pinSetter.pins)
            {
                if (p.GetComponent<Pin>().hit)
                {
                    sensor.AddObservation(0f);
                    sensor.AddObservation(new Vector3(-10000f, -10000f, -10000f).normalized);
                }
                else
                {
                    sensor.AddObservation(1f);
                    sensor.AddObservation(p.GetComponent<Pin>().transform.localPosition.normalized);
                }
                    
            }

            try
            {
               
                if (_playerAbilities.isBallRolling)
                {
                    BowlingBall bowlingBall = _playerAbilities.bowlingBall.GetComponent<BowlingBall>();
                    // Track ball position and speed
                    sensor.AddObservation(bowlingBall.transform.forward.normalized);
                    sensor.AddObservation(bowlingBall.rb.velocity.normalized);
                    sensor.AddObservation(bowlingBall.isInGutter ? 1.0f : 0.0f);
                }
                else
                {
                    sensor.AddObservation(new Vector3(-10000f, -10000f, -10000f).normalized);
                    sensor.AddObservation(new Vector3(-10000f, -10000f, -10000f).normalized);
                    sensor.AddObservation(0f);
                }
             
            }
            catch (UnassignedReferenceException)
            {
                sensor.AddObservation(new Vector3(-10000f, -10000f, -10000f).normalized);
                sensor.AddObservation(new Vector3(-10000f, -10000f, -10000f).normalized);
                sensor.AddObservation(0f);
            }
        
            // Add past actions to observations
            foreach (AgentAction pastAction in lastActions)
            {
                sensor.AddObservation(pastAction.move.normalized);
                sensor.AddObservation(pastAction.aim.normalized);
            }

        }
        catch (MissingReferenceException) { }
        catch (IndexOutOfRangeException)
        {
            Debug.Log($"currentFrame {bowler.currentFrame} : currentRoll {bowler.currentRoll} : {bowler.playerObject}");
        }

    }


  
    public override void OnEpisodeBegin()
    {
        //Debug.Log("OnEpisodeBegin");

        episodeReward = 0;
    }
}

public class AgentAction
{
    public Vector2 move;
    public Vector2 aim;
}
