protoc --proto_path=./Proto/src  --csharp_out=../Assets/Script/NetFramework/proto ./Proto/src/*.proto
protoc --proto_path=./Proto/src --go_out=./ServerGen/proto ./Proto/src/*.proto

python3 gen_proto.py
pause