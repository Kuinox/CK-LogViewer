import { connect, IPublishPacket, MqttClient } from "mqtt";

export class MQTTService {
    private mqttClient: MqttClient;
    private registerMap: {
        [key: string]: ((packet: IPublishPacket) => void)[];
    } = {};
    /**
     *
     */
    constructor(endpoint: string) {
        this.mqttClient = connect("ws://"+endpoint);
        this.mqttClient.on("message", this.onMessage);
    }

    private onMessage = (topic: string, payload: Buffer, packet: IPublishPacket) => {
        const events = this.registerMap[topic.split("/")[1]];
        if(events != undefined) {
            for (let i = 0; i < events.length; i++) {
                const element = events[i];
                element(packet);
            }
        }
    };

    public async listenTo(logName: string, onMessage:(packet: IPublishPacket) => void): Promise<() => void> {
        if(this.registerMap[logName] == undefined) this.registerMap[logName] = [];
        this.registerMap[logName].push(onMessage);
        await new Promise<void>(
            (resolve, reject) => {
                console.log("subscribing to logLive/" + logName);
                this.mqttClient.subscribe("logLive/" + logName, {
                    qos: 2
                }, (err, granted) => {
                    console.log("got response");
                    console.log(granted);
                    if(err != null) {
                        reject(err);
                    }
                    if(granted.length > 0 ) {
                        resolve();
                    } else {
                        reject(new Error("Could not subscribe."));
                    }
                });
            }
        );
        return () => {
            this.stopListening(logName);
            const arr =this.registerMap[logName];
            arr.splice(arr.indexOf(onMessage), 1);
        };
    }

    private stopListening(logName: string) : Promise<void> {
        return new Promise<void>(
            (resolve, reject) => {
                this.mqttClient.unsubscribe("logLive/" + logName, {
                    qos: 2
                }, (err, granted) => {
                    if(err != null) {
                        reject(err);
                    }
                    resolve();
                });
            }
        );
    }
}