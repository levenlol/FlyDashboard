
class MultiButton
{
public:

    MultiButton()
    {
        analogPin = 255;
        readNum = 0;
    }
    
    MultiButton(uint8_t inAnalogPin, unsigned int inReadValues[], unsigned int inReadValuesNum)
    {
        analogPin = inAnalogPin;
        readNum = inReadValuesNum;

        memcpy(readValues, inReadValues, sizeof(unsigned int) * inReadValuesNum);
    }

private:
    uint8_t analogPin;
    unsigned int readNum;
    unsigned int readValues[];
};