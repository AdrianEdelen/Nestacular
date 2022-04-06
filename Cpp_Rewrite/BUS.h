namespace NES {
    class BUS {
        public:
        unsigned char RAM[64 * 1024];
        void Write(unsigned short address, unsigned char data);  
        unsigned char Read(unsigned short address, unsigned char data, bool readOnly = false);   
    };
}