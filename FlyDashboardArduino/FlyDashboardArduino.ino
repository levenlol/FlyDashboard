#include <LiquidCrystal.h>

static constexpr int switchActionPin = 7;
static constexpr int commandLength = 5; 
static constexpr int screenSize = 16;

static constexpr char startCmdChar = '$';
static constexpr char endCmdChar = '#';

LiquidCr6ystal lcd(12, 11, 5, 4, 3, 2);

struct SimData
{
    long Altitude = 0;
    unsigned int GroundSpeed = 0;
    unsigned int AirSpeed = 0;
    unsigned int Heading = 0;
};

enum State
{
    WAITING = 0,
    READ = 1,
    WRITE = 2
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


void setup()
{
    pinMode(switchActionPin, INPUT);

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

    if(currentState != State::WAITING)
    {
        // Check if action button is pressed.
        if(digitalRead(switchActionPin) == HIGH)
        {
            const Action newAction = static_cast<Action>(max(1, (static_cast<int>(currentAction) + 1) % _MAX));
            ChangeAction(newAction);

            delay(250);
        }
        else if(IsActionSelected) // update every 250 ms. dont delay.
        {
            const long now = millis();
            if(now - actionTimer > 250)
            {
                HandleSelectedAction();
                actionTimer = now;
            }
        }
    }
}

bool IsActionSelected()
{
    return currentAction > Action::_NONE && currentAction < Action::_MAX;
}

void HandleSelectedAction()
{
    if(currentAction == Action::ALTITUDE)
    {
        // build altitude str, want a fixed size string (big performance improvement on display on led.)
        const String altitudeStr = BuildStringOfSizeFromNumber(simData.Altitude, 5);
        
        PrintOnLCD(altitudeStr, screenSize - 5, false);
    }
    else if(currentAction == Action::SPEED)
    {
        // convert from feet/s to knots.
        String speedStr = BuildStringOfSizeFromNumber(simData.AirSpeed / 1.688, 4);

        speedStr.concat(" kn");
        PrintOnLCD(speedStr, screenSize - 7, false); // padding of 7, 4 (length of speed) + 3(unit measure)
    }
    else if(currentAction == Action::HEADING)
    {
        String headingStr = BuildStringOfSizeFromNumber(simData.Heading, 3);

        PrintOnLCD(headingStr, screenSize - 3, false);
    }
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
    }
    else if(currentAction == Action::SPEED)
    {
        PrintOnLCD("Speed: ");
    }
    else if (currentAction == Action::HEADING)
    {
        PrintOnLCD("HEADING: ");
    }
}

void ChangeState(State newState)
{
    currentState = newState;
    if(currentState == State::WAITING)
    {
        PrintOnLCD("Waiting for\nConnection...");
    }
    else if (currentState == State::READ)
    {
        PrintOnLCD("");
    }
    else if (currentState == State::WRITE)
    {
        PrintOnLCD("");
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
    if(cmd.equals("ALTIT")) // altitude
    {
        simData.Altitude = content.toInt();
    }
    else if (cmd.equals("GROSP")) // ground speed
    {
        simData.GroundSpeed = content.toInt();
    }
    else if (cmd.equals("AIRSP")) // air speed
    {
        simData.AirSpeed = content.toInt();
    }
    else if(cmd.equals("HEADI")) // heading
    {
        simData.Heading = content.toInt();
    }
    else if(cmd.equals("CONNE"))
    {
        ChangeAction(Action::_NONE);
        ChangeState(State::READ);

        PrintOnLCD("Connected and \nReady to ROCK !");
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
    PrintOnLCD(msg, 0, true);
}

void PrintOnLCD(const String& msg, uint8_t startIndex, bool bClear)
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

        lcd.setCursor(startIndex, 0);

        lcd.print(row1);

        lcd.setCursor(startIndex, 1);
        lcd.print(row2);
    }
    else
    {
        lcd.setCursor(startIndex, 0);
        lcd.print(msg);
    }
}