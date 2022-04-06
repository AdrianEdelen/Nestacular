#include <iostream>
#include "BUS.h"
using namespace std;
using namespace NES;
int main() {
  cout << "Hello World!";
  return 0;

    BUS bus;

    bus.Write(0x0001, 0x01);

}