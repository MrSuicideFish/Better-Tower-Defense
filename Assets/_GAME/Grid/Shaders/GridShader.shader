Shader "Unlit/GridShader"
{
    
    Properties
    {
        _noiseMap("Noise Map", 2D) = "white" {}
        _colorNoiseMap("Color Noise Map", 2D) = "white" {}
        _gridColor("Grid Color", Color) = (1,1,1,1)
        _vacantColor("Vacant Color", Color) = (1,1,1,1)
        _occupiedColor("Occupied Color", Color) = (1,1,1,1)
        _unbuildableColor("Unbuildable Color", Color) = (1,1,1,1)
        _highlightColor("Highlight Color", Color) = (1,1,1,1)
        _gridOffset("Grid Offset", float) = 0.0
        _lineWidth("Line Width", float) = 0.0
        _gridSize("Grid Size", float) = 0.0
        _lineDepth("Line Depth", float) = 0.0
        _waveSpeed("Wave Speed", float) = 0.0
        _waveHeight("Wave Height", float) = 0.0
        _waveColorOffset("Wave Color Offset", float) = 0.0
        _gridPositionDistancePow("Grid Slot Dist Power", float) = 0.0
        _gridPositionDistanceFalloff("Grid Slot Dist Falloff", float) = 0.0
        _gridColorLerpPower("Grid Color Lerp Power", float) = 0.0
        _gridColorPower("Grid Color Power", float) = 0.0
        _colorWaveSpeed("Grid Color Wave Speed", float) = 0.0
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        LOD 100
        
        ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha 

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            sampler2D _noiseMap;
            sampler2D _colorNoiseMap;
            fixed4 _gridColor;
            fixed4 _vacantColor;
            fixed4 _occupiedColor;
            fixed4 _unbuildableColor;
            fixed4 _highlightColor;
            float _lineWidth;
            float _lineDepth;
            float _gridSize;
            float _gridOffset;
            float _waveSpeed;
            float _waveHeight;
            float _waveOffset;
            float _waveColorOffset;
            float _gridPositionDistancePow;
            float _gridPositionDistanceFalloff;
            float _gridColorLerpPower;
            float _gridColorPower;
            float _colorWaveSpeed;

            float4 _mouseCoordPosition;
            float4 _vacantPositions[50];
            float4 _occupiedPositions[50];
            float4 _unbuildablePositions[50];

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD2;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            float getManhattanDist(float3 from, float3 to)
            {
                //return distance(from, to);
                return abs(from.z - to.z) + abs(from.x - to.x);
            }
            
            float4 getClosestVacantSpace(float3 coord)
            {
                float closestDist = 9999;
                float4 closestCoord;
                for(int i = 0; i < 50; i++)
                {
                    float dist = getManhattanDist(coord, _vacantPositions[i]);
                    if(dist < closestDist)
                    {
                        closestCoord = _vacantPositions[i];
                        closestDist = dist; 
                    }
                }

                return closestCoord;
            }

            float4 getClosestOccupiedSpace(float3 coord)
            {
                float closestDist = 9999;
                float4 closestCoord;
                for(int i = 0; i < 50; i++)
                {
                    float dist = getManhattanDist(coord, _occupiedPositions[i]);
                    if(dist < closestDist)
                    {
                        closestCoord = _occupiedPositions[i];
                        closestDist = dist; 
                    }
                }

                return closestCoord;
            }

            float4 getClosestUnbuildableSpace(float3 coord)
            {
                float closestDist = 9999;
                float4 closestCoord;
                for(int i = 0; i < 50; i++)
                {
                    float dist = getManhattanDist(coord, _unbuildablePositions[i]);
                    if(dist < closestDist)
                    {
                        closestCoord = _unbuildablePositions[i];
                        closestDist = dist; 
                    }
                }

                return closestCoord;
            }

            v2f vert (appdata v)
            {
                _waveOffset = (_waveOffset + (_Time.r * _waveSpeed)) % 10;
                v2f o;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex); 
                o.uv = v.uv;
                o.vertex = UnityObjectToClipPos(v.vertex);

                fixed4 noise = tex2Dlod(_noiseMap, float4(v.uv / (_gridSize*2), 0,0) * _waveOffset) * _waveHeight;
                o.vertex.y = (o.vertex.y + noise.r);

                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = _gridColor;
                float2 center = float2(
                    (int)(round(abs(i.worldPos.x)/_gridSize) * _gridSize),
                    (int)(round(abs(i.worldPos.z)/_gridSize) * _gridSize));

                //float centerDist = lerp(0,1,distance(i.worldPos.xz, center));
                float centerDist = abs(i.worldPos.z - center.y) + abs(i.worldPos.x - center.x);
                
                fixed4 noise = tex2D(_noiseMap, float4(i.uv*_waveColorOffset+(_Time.r * _colorWaveSpeed), 0,0));
                fixed4 colorNoise = tex2D(_colorNoiseMap, float4(i.uv*_waveColorOffset+(_Time.r * _colorWaveSpeed), 0,0)); 
                float4 closestVacant = getClosestVacantSpace(i.worldPos);
                float4 closestOccupied = getClosestOccupiedSpace(i.worldPos);
                float4 closestUnbuildable = getClosestUnbuildableSpace(i.worldPos);

                bool noLine = false;
                if(abs(i.worldPos.x + _gridOffset) % _gridSize > _lineWidth
                    && abs(i.worldPos.z + _gridOffset) % _gridSize > _lineWidth
                    && abs(i.worldPos.x - _gridOffset) % _gridSize < 1 - _lineWidth
                    && abs(i.worldPos.z - _gridOffset) % _gridSize < 1 - _lineWidth
                    || (abs(i.worldPos.x - _gridOffset) % _gridSize < _lineWidth
                    && abs(i.worldPos.z - _gridOffset) % _gridSize < _lineWidth)
                    || (abs(i.worldPos.x + _gridOffset) % _gridSize < _lineWidth
                        && abs(i.worldPos.z + _gridOffset) % _gridSize < _lineWidth))
                {
                    col.a = _lineDepth - _gridColor.a;
                    noLine = true;
                }

                float vacantDist = getManhattanDist(i.worldPos, closestVacant) * _gridPositionDistancePow;
                float vacantDistPerc = (vacantDist/_gridPositionDistanceFalloff);
                float vacantDistAlpha = clamp(1 - lerp(0,1,vacantDistPerc), 0, 1);

                float occupiedDist = getManhattanDist(i.worldPos, closestOccupied) * _gridPositionDistancePow;
                float occupiedDistPerc = (occupiedDist/_gridPositionDistanceFalloff);
                float occupiedDistAlpha = clamp(1 - lerp(0,1,occupiedDistPerc), 0, 1);

                float unbuildableDist = getManhattanDist(i.worldPos, closestUnbuildable) * _gridPositionDistancePow;
                float unbuildableDistPerc = unbuildableDist/_gridPositionDistanceFalloff;
                float unbuildableDistAlpha = clamp(1 - lerp(0,1,unbuildableDistPerc), 0, 1);

                bool isSpecialSquare = false;
                half4 targetColor = _gridColor;
                float targetDistAlpha = 0;
                if(vacantDist <= _gridPositionDistanceFalloff
                    || occupiedDist <= _gridPositionDistanceFalloff
                    || unbuildableDist <= _gridPositionDistanceFalloff)
                {
                    isSpecialSquare = true;
                    if(vacantDist < occupiedDist && vacantDist < unbuildableDist)
                    {
                        targetColor = _vacantColor;
                        targetDistAlpha = vacantDistAlpha;
                    }
                    else if(occupiedDist < vacantDist && occupiedDist < unbuildableDist)
                    {
                        targetColor = _occupiedColor;
                        targetDistAlpha = occupiedDistAlpha;
                    }
                    else if(unbuildableDist < vacantDist && unbuildableDist < occupiedDist)
                    {
                        targetColor = _unbuildableColor;
                        targetDistAlpha = unbuildableDistAlpha;
                    }
                }

                float totalAlpha = clamp(vacantDistAlpha + occupiedDistAlpha + unbuildableDistAlpha, 0, 1);
                if(targetDistAlpha == unbuildableDistAlpha
                    || targetDistAlpha == occupiedDist
                    || targetDistAlpha == vacantDistAlpha)
                {
                    if(isSpecialSquare)
                    {
                        col.rgb -= lerp(_gridColor, targetColor, targetDistAlpha);    
                    }else
                    {
                        col.rgb = _gridColor;
                    }
                    
                }else
                {
                    col.rgb = lerp(_gridColor, targetColor, targetDistAlpha + _gridColorLerpPower);
                }

                // apply colors
                col.a /= (centerDist / noise);
                col.rgb *= mul(col.rgb, _gridColorPower) * noise;

                if(isSpecialSquare)
                { 
                    col.a = (totalAlpha * 10) + (colorNoise * .2);
                }else
                {
                    col.a *= colorNoise * 3;
                }

                /// do mouse coord highlight
                if(distance(_mouseCoordPosition, float4(0,0,0,0)) != 0 && isSpecialSquare)
                {
                    float size = _gridSize;
                    int coordX = (int)(round((i.worldPos.x / size)) * size);
                    int coordY = (int)(round((i.worldPos.z / size)) * size);

                    float3 mouseCoord = float3(_mouseCoordPosition.x, 0, _mouseCoordPosition.z);
                    float3 tileCoord = float3(coordX, 0, coordY);
                    float mouseToCoordDist = getManhattanDist(mouseCoord, tileCoord);

                    bool isTile = coordX == _mouseCoordPosition.x && coordY == _mouseCoordPosition.z;
                    if(mouseToCoordDist <= 1 && isTile && noLine && targetDistAlpha != unbuildableDistAlpha && targetDistAlpha != occupiedDistAlpha)
                    {
                        col.rgb += _highlightColor.rgb * _highlightColor.a * _gridSize;
                    }

                }
                if(col.a < 0) col.a = 0;
                
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
