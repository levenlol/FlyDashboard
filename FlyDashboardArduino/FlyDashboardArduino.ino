#include <LiquidCrystal.h>
#include "MultiButton.h"

static constexpr uint8_t headingButtonPin = 6;
static constexpr uint8_t switchActionPin = 7;
static constexpr uint8_t autopilotStatePin = 8;
static constexpr int commandLength = 5; 
static constexpr int screenSize = 16;

static constexpr uint8_t headingTrimmerPin = A0;
static constexpr uint8_t altitudeTrimmerPin = A1;


static constexpr char startCmdChar = '$';
static constexpr char endCmdChar = '#';

LiquidCrystal lcd(10, 9, 5, 4, 3, 2);

struct SimData
{
    unsigned int GroundSpeed = 0;
    unsigned int AirSpeed = 0;
    unsigned int VerticalSpeed = 0;
};

struct APData
{
    unsigned int heading = 0;
    unsigned long altitude = 2000;
};

enum State
{
    WAITING = 0,
    WRITE = 1
};

enum Action
{
    _NONE = 0,
    ALTITUDE = 1,
    SPEED = 2,
    HEADING = 3,
    _MAX
};


Action currentAction = Action::_NONE;
State currentState = State::WAITING;
SimData simData;
APData apData;


void setup()
{
    pinMode(switchActionPin, INPUT);
    pinMode(autopilotStatePin, INPUT);
    pinMode(headingButtonPin, INPUT);

    Init_LCD();
    Init_Serial();

    ChangeState(State::WAITING);
}

void Init_LCD()
{
    lcd.begin(16, 2);
}

void Init_Serial()
{
    Serial.begin(9600);
}

void loop()
{
    static long actionTimer = 0;
    static long writeTimer = 0;

    if(currentState != State::WAITING)
    {
        // First Check if buttons are pressed. (user input)
        if(digitalRead(switchActionPin) == HIGH)
        {
            const Action newAction = static_cast<Action>(max(1, (static_cast<int>(currentAction) + 1) % _MAX));
            ChangeAction(newAction);

            delay(250);
        }
        
        
        if(currentState == State::WRITE)
        {
            HandleWriteAction();

            const long now = millis();
            if(now - writeTimer > 250)
            {
                if(digitalRead(headingButtonPin) == HIGH)
                {
                    Serial.println("THEAD-.$");
                }
                else if(digitalRead(autopilotStatePin) == HIGH)
                {
                    Serial.println("TAUTO-.$");
                }

                writeTimer = millis();
            }
        }
    }
}

bool IsActionSelected()
{
    return currentAction > Action::_NONE && currentAction < Action::_MAX;
}

void UpdateLCDValue(long value, uint8_t dim = 5, uint8_t rowindex = 0)
{
    const String valStr = BuildStringOfSizeFromNumber(value, dim);
    PrintOnLCD(valStr, screenSize - dim, rowindex, false);
}

void HandleWriteAction()
{
    //if(IsActionSelected())
    {
        {
            // Handle Heading
            const int trimmerValue = analogRead(headingTrimmerPin);

            const int headingValue = map(trimmerValue, 0, 1023, 0, 359);

            if(apData.heading != headingValue)
            {
                apData.heading = headingValue;

                if(currentAction == Action::HEADING)
                {
                    UpdateLCDValue(headingValue);
                }

                SendCommand("SHEAD", String(apData.heading));
            }
        }

        {
            const int trimmerValue = analogRead(altitudeTrimmerPin);

            const long altitudeValue = map(trimmerValue, 0, 1023, 0, 800) * 100;

            if(apData.altitude != altitudeValue)
            {
                apData.altitude = altitudeValue;

                if(currentAction == Action::ALTITUDE)
                {
                    UpdateLCDValue(altitudeValue, 7);
                }

                SendCommand("SALTI", String(apData.altitude));
            }
        }
    }
}

void SendCommand(String command, String content)
{
    Serial.print(command + String("-") + content + String("$")) ;
}

// Build a string with size N. if digit number is less than size, string is pad-filled with spaces.
String BuildStringOfSizeFromNumber(unsigned long value, unsigned int size)
{
    String str;

    str.reserve(size);
    
    for(int i = 0; i < size; i++)
        str.concat(' ');

    int i = 0;
    int length = str.length();
    do
    {
        const long digit = value % 10;
        str.setCharAt(length - i - 1, '0' + digit);

        value /= 10;

        i++;
    } while(value > 0);

    return str;
}

void ChangeAction(Action newAction)
{
    currentAction = newAction;
    if(currentAction == Action::ALTITUDE)
    {
        PrintOnLCD("Altitude: ");
        PrintOnLCD("VS: ", 0, 1, false);
        UpdateLCDValue(apData.altitude, 6);
    }
    else if(currentAction == Action::SPEED)
    {
        PrintOnLCD("Speed: ");
    }
    else if (currentAction == Action::HEADING)
    {
        PrintOnLCD("Heading: ");
        UpdateLCDValue(apData.heading);
    }
}

void ChangeState(State newState)
{
    currentState = newState;
    if(currentState == State::WAITING)
    {
        PrintOnLCD("Waiting for\nConnection...");
    }
    else if (currentState == State::WRITE)
    {
        ChangeAction(currentAction);
    }
}

void serialEvent()
{
    while (Serial.available()) 
    {
        // get the new byte:
        char inChar = (char)Serial.read();
        if(inChar == startCmdChar)
        {
            const String cmd = Serial.readStringUntil(endCmdChar);
            HandleCommand(cmd);
        }
        else
        {
            PrintOnLCD("Error: " + inChar);
        }
    }
}

void HandleCommand(String msg)
{
    const String cmd = msg.substring(0, commandLength);
    const String content = msg.substring(commandLength);
    
    if (cmd.equals("GROSP")) // ground speed
    {
        simData.GroundSpeed = content.toInt();
    }
    else if (cmd.equals("AIRSP")) // air speed
    {
        simData.AirSpeed = content.toInt();
    }
    else if(cmd.equals("CONNE"))
    {
        ChangeAction(Action::_NONE);
        ChangeState(State::WRITE);

        PrintOnLCD("Connected and \nReady to ROCK !");

        delay(1000);

        ChangeAction(Action::ALTITUDE);

    }
    else if(cmd.equals("DISCO"))
    {
        ChangeAction(Action::_NONE);
        ChangeState(State::WAITING); 

        PrintOnLCD("Disconnected.\nSee you soon!");
    }
}

void PrintOnLCD(String msg)
{
    PrintOnLCD(msg, 0, 0, true);
}

void PrintOnLCD(const String& msg, uint8_t startIndex, uint8_t rowIndex, bool bClear)
{
    if(bClear)
    {
        lcd.clear();
    }

    const int index = msg.indexOf('\n');
    if(index >= 0)
    {
        const String row1 = msg.substring(0, index);
        const String row2 = msg.substring(index + 1);

        lcd.setCursor(startIndex, rowIndex);

        lcd.print(row1);
        rowIndex++;

        lcd.setCursor(startIndex, rowIndex);
        lcd.print(row2);
    }
    else
    {
        lcd.setCursor(startIndex, rowIndex);
        lcd.print(msg);
    }
}