using AutoMapper;
using HelpDesk.Models.ViewModels;
using HelpDesk.Models.Entities;

namespace HelpDesk.Web
{
    public class Profiles
    {
        //AutoMapper kullanabilmek amacıyla, maplenecek her model için Profile'dan kalıtım almış classlar kullanılmalı.
        public class FailureProfile : Profile
        {
            public FailureProfile()
            {
                CreateMap<Failure, FailureViewModel>().ReverseMap();
            }
        }

        public class FailureLogProfile : Profile
        {
            public FailureLogProfile()
            {
                CreateMap<FailureLog, FailureLogViewModel>().ReverseMap();
            }
        }

        //Çoğaltılabilir...
        //Controller'da kullanmak için IMapper ile DI yapılmalı.
    }
}
