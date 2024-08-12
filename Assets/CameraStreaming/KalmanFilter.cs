using UnityEngine;

public class KalmanFilter
{
    private float Q = 0.0001f; // Process noise covariance
    private float R = 0.01f;   // Measurement noise covariance
    private float P = 1, K;    // Error covariance and Kalman gain
    private float X;           // Estimated value

    public float Update(float measurement)
    {
        // Prediction update
        P = P + Q;

        // Measurement update
        K = P / (P + R);
        X = X + K * (measurement - X);
        P = (1 - K) * P;

        return X;
    }
}